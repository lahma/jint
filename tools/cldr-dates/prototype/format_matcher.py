"""Reference prototype for sebastienros/jint#4158 - NOT shipped, NOT run by the build.

A Python model of the ECMA-402 BestFitFormatMatcher as ICU's DateTimePatternGenerator (which V8 and
SpiderMonkey call) implements it, over CLDR 48.2's resolved JSON (cldr-dates-full + cldr-core 48.2.0).
It exists to validate the algorithm the C# port will implement before any C# is written: run it over a
set of locales and option bags and compare its output with an ICU-based engine (Node 24.19 = ICU 78.3 /
CLDR 48.0 was the reference while designing).

Usage:
  python format_matcher.py <cldr-json-root> <probe-tsv-from-node> [--basic]

<cldr-json-root> holds `package/` (cldr-dates-full) and `core/package/` (cldr-core). The probe TSV is
the output of probe.js run under Node; the bags below must stay in step with it.

What it models, in ICU's own terms (icu4c/source/i18n/dtptngen.cpp):
  * skeleton construction from the options bag, the way V8's js-date-time-format.cc builds it;
  * DateTimeMatcher::set (field types, the implied 'a' of a 12-hour skeleton) and getDistance;
  * getBestRaw over availableFormats + the date/time style patterns + the canonical single fields;
  * getBestAppending (date and time halves, appendItems, the fractional-second fix-up);
  * the dateTimeFormat choice by month width (atTime variants for long and full, as ICU 72+ does);
  * adjustFieldTypes with UDATPG_MATCH_HOUR_FIELD_LENGTH (V8's option) and the specified-skeleton rule;
  * V8's hour-cycle replacement in the chosen pattern.
With --basic it runs the ECMA-402 BasicFormatMatcher instead (spec penalties), for comparison.
"""
import io
import json
import os
import sys

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# ---------------------------------------------------------------------------------------------------
# ICU field indices (UDateTimePatternField order) and the dtTypes table.
ERA, YEAR, QUARTER, MONTH, WOY, WOM, WEEKDAY, DOY, DOWIM, DAY, DAYPERIOD, HOUR, MINUTE, SECOND, FRACSEC, ZONE = range(16)
FIELD_COUNT = 16
DATE_MASK = (1 << DAYPERIOD) - 1
TIME_MASK = ((1 << FIELD_COUNT) - 1) & ~DATE_MASK
APPEND_KEYS = {ERA: 'Era', YEAR: 'Year', QUARTER: 'Quarter', MONTH: 'Month', WOY: 'Week', WOM: 'Week', WEEKDAY: 'Day-Of-Week',
               DOY: 'Day', DOWIM: 'Day', DAY: 'Day', DAYPERIOD: None, HOUR: 'Hour', MINUTE: 'Minute', SECOND: 'Second',
               FRACSEC: 'Second', ZONE: 'Timezone'}
FIELD_NAME_KEYS = {ERA: 'era', YEAR: 'year', QUARTER: 'quarter', MONTH: 'month', WOY: 'week', WOM: 'weekOfMonth',
                   WEEKDAY: 'weekday', DOY: 'dayOfYear', DOWIM: 'weekdayOfMonth', DAY: 'day', DAYPERIOD: 'dayperiod',
                   HOUR: 'hour', MINUTE: 'minute', SECOND: 'second', FRACSEC: 'second', ZONE: 'zone'}

DT_NARROW, DT_SHORTER, DT_SHORT, DT_LONG, DT_NUMERIC, DT_DELTA = -0x101, -0x102, -0x103, -0x104, 0x100, 0x10

# (char, field, type, minLen)
DT_TYPES = [
    ('G', ERA, DT_SHORT, 1), ('G', ERA, DT_LONG, 4), ('G', ERA, DT_NARROW, 5),
    ('y', YEAR, DT_NUMERIC, 1), ('Y', YEAR, DT_NUMERIC + DT_DELTA, 1), ('u', YEAR, DT_NUMERIC + 2 * DT_DELTA, 1),
    ('r', YEAR, DT_NUMERIC + 3 * DT_DELTA, 1), ('U', YEAR, DT_SHORT, 1), ('U', YEAR, DT_LONG, 4), ('U', YEAR, DT_NARROW, 5),
    ('Q', QUARTER, DT_NUMERIC, 1), ('Q', QUARTER, DT_SHORT, 3), ('Q', QUARTER, DT_LONG, 4), ('Q', QUARTER, DT_NARROW, 5),
    ('q', QUARTER, DT_NUMERIC + DT_DELTA, 1), ('q', QUARTER, DT_SHORT - DT_DELTA, 3), ('q', QUARTER, DT_LONG - DT_DELTA, 4),
    ('q', QUARTER, DT_NARROW - DT_DELTA, 5),
    ('M', MONTH, DT_NUMERIC, 1), ('M', MONTH, DT_SHORT, 3), ('M', MONTH, DT_LONG, 4), ('M', MONTH, DT_NARROW, 5),
    ('L', MONTH, DT_NUMERIC + DT_DELTA, 1), ('L', MONTH, DT_SHORT - DT_DELTA, 3), ('L', MONTH, DT_LONG - DT_DELTA, 4),
    ('L', MONTH, DT_NARROW - DT_DELTA, 5), ('l', MONTH, DT_NUMERIC + DT_DELTA, 1),
    ('w', WOY, DT_NUMERIC, 1), ('W', WOM, DT_NUMERIC, 1),
    ('E', WEEKDAY, DT_SHORT, 1), ('E', WEEKDAY, DT_LONG, 4), ('E', WEEKDAY, DT_NARROW, 5), ('E', WEEKDAY, DT_SHORTER, 6),
    ('c', WEEKDAY, DT_NUMERIC + 2 * DT_DELTA, 1), ('c', WEEKDAY, DT_SHORT - 2 * DT_DELTA, 3), ('c', WEEKDAY, DT_LONG - 2 * DT_DELTA, 4),
    ('c', WEEKDAY, DT_NARROW - 2 * DT_DELTA, 5), ('c', WEEKDAY, DT_SHORTER - 2 * DT_DELTA, 6),
    ('e', WEEKDAY, DT_NUMERIC + DT_DELTA, 1), ('e', WEEKDAY, DT_SHORT - DT_DELTA, 3), ('e', WEEKDAY, DT_LONG - DT_DELTA, 4),
    ('e', WEEKDAY, DT_NARROW - DT_DELTA, 5), ('e', WEEKDAY, DT_SHORTER - DT_DELTA, 6),
    ('d', DAY, DT_NUMERIC, 1), ('g', DAY, DT_NUMERIC + DT_DELTA, 1), ('D', DOY, DT_NUMERIC, 1), ('F', DOWIM, DT_NUMERIC, 1),
    ('a', DAYPERIOD, DT_SHORT, 1), ('a', DAYPERIOD, DT_LONG, 4), ('a', DAYPERIOD, DT_NARROW, 5),
    ('b', DAYPERIOD, DT_SHORT - DT_DELTA, 1), ('b', DAYPERIOD, DT_LONG - DT_DELTA, 4), ('b', DAYPERIOD, DT_NARROW - DT_DELTA, 5),
    ('B', DAYPERIOD, DT_SHORT - 3 * DT_DELTA, 1), ('B', DAYPERIOD, DT_LONG - 3 * DT_DELTA, 4), ('B', DAYPERIOD, DT_NARROW - 3 * DT_DELTA, 5),
    ('H', HOUR, DT_NUMERIC + 10 * DT_DELTA, 1), ('k', HOUR, DT_NUMERIC + 11 * DT_DELTA, 1), ('h', HOUR, DT_NUMERIC, 1),
    ('K', HOUR, DT_NUMERIC + DT_DELTA, 1),
    ('m', MINUTE, DT_NUMERIC, 1), ('s', SECOND, DT_NUMERIC, 1), ('A', SECOND, DT_NUMERIC + DT_DELTA, 1),
    ('S', FRACSEC, DT_NUMERIC, 1),
    ('v', ZONE, DT_SHORT - 2 * DT_DELTA, 1), ('v', ZONE, DT_LONG - 2 * DT_DELTA, 4), ('z', ZONE, DT_SHORT, 1), ('z', ZONE, DT_LONG, 4),
    ('Z', ZONE, DT_NARROW - DT_DELTA, 1), ('Z', ZONE, DT_LONG - DT_DELTA, 4), ('Z', ZONE, DT_SHORT - DT_DELTA, 5),
    ('O', ZONE, DT_SHORT - DT_DELTA, 1), ('O', ZONE, DT_LONG - DT_DELTA, 4),
    ('V', ZONE, DT_SHORT - DT_DELTA, 1), ('V', ZONE, DT_LONG - DT_DELTA, 2), ('V', ZONE, DT_LONG - 1 - DT_DELTA, 3),
    ('V', ZONE, DT_LONG - 2 - DT_DELTA, 4),
    ('X', ZONE, DT_NARROW - DT_DELTA, 1), ('X', ZONE, DT_SHORT - DT_DELTA, 2), ('X', ZONE, DT_LONG - DT_DELTA, 4),
    ('x', ZONE, DT_NARROW - DT_DELTA, 1), ('x', ZONE, DT_SHORT - DT_DELTA, 2), ('x', ZONE, DT_LONG - DT_DELTA, 4),
]


def canonical_row(ch, length):
    best = None
    for row in DT_TYPES:
        if row[0] == ch and row[3] <= length:
            if best is None or row[3] > best[3]:
                best = row
    return best


def tokenize(pattern):
    """Split an LDML pattern into ('field', ch, len) / ('lit', text) tokens; quotes are kept raw in lit."""
    out = []
    i = 0
    n = len(pattern)
    while i < n:
        c = pattern[i]
        if c == "'":
            j = i + 1
            text = "'"
            while j < n:
                if pattern[j] == "'":
                    if j + 1 < n and pattern[j + 1] == "'":
                        text += "''"
                        j += 2
                        continue
                    text += "'"
                    j += 1
                    break
                text += pattern[j]
                j += 1
            out.append(('lit', text))
            i = j
            continue
        if ('a' <= c <= 'z') or ('A' <= c <= 'Z'):
            j = i
            while j < n and pattern[j] == c:
                j += 1
            out.append(('field', c, j - i))
            i = j
            continue
        out.append(('lit', c))
        i += 1
    return out


class Skeleton:
    def __init__(self, text):
        self.text = text
        self.orig = {}   # field -> (char, len)
        self.type = [0] * FIELD_COUNT
        for tok in tokenize(text):
            if tok[0] != 'field':
                continue
            row = canonical_row(tok[1], tok[2])
            if row is None:
                continue
            f = row[1]
            self.orig[f] = (tok[1], tok[2])
            t = row[2]
            if t > 0:
                t += tok[2]
            self.type[f] = t
        # DateTimeMatcher::set: a 12-hour skeleton without a day period gets the default 'a'; a 24-hour one loses it.
        if HOUR in self.orig:
            hc = self.orig[HOUR][0]
            if hc in 'hK':
                if DAYPERIOD not in self.orig:
                    self.orig[DAYPERIOD] = ('a', 1)
                    self.type[DAYPERIOD] = DT_SHORT
            elif DAYPERIOD in self.orig:
                del self.orig[DAYPERIOD]
                self.type[DAYPERIOD] = 0

    def mask(self):
        m = 0
        for f in range(FIELD_COUNT):
            if self.type[f] != 0:
                m |= 1 << f
        return m


EXTRA_FIELD, MISSING_FIELD = 0x10000, 0x1000


def distance(req, include_mask, cand):
    result = 0
    missing = 0
    extra = 0
    for f in range(FIELD_COUNT):
        my = req.type[f] if include_mask & (1 << f) else 0
        other = cand.type[f]
        if my == other:
            continue
        if my == 0:
            result += EXTRA_FIELD
            extra |= 1 << f
        elif other == 0:
            result += MISSING_FIELD
            missing |= 1 << f
        else:
            result += abs(my - other)
    return result, missing, extra


# ---------------------------------------------------------------------------------------------------
class LocaleData:
    def __init__(self, root, locale):
        self.locale = locale
        main = os.path.join(root, 'package', 'main', locale)
        g = json.load(open(os.path.join(main, 'ca-gregorian.json'), encoding='utf-8'))['main'][locale]['dates']['calendars']['gregorian']
        self.g = g
        fields = json.load(open(os.path.join(root, 'package', 'main', locale, 'dateFields.json'), encoding='utf-8'))
        self.fields = fields['main'][locale]['dates']['fields']
        dtf = g['dateTimeFormats']
        self.append_items = dtf['appendItems']
        self.dt_formats = {k: dtf[k] for k in ('full', 'long', 'medium', 'short')}
        at = g.get('dateTimeFormats-atTime', {}).get('standard', {})
        self.dt_formats_at = {k: at.get(k, self.dt_formats[k]) for k in self.dt_formats}
        # Candidates: skeleton text -> pattern. ICU order: style patterns, canonical items, then availableFormats
        # overriding any skeleton already present.
        cands = {}
        bases = set()
        def add(pattern, skel=None, override=False):
            # DateTimePatternGenerator::addPatternWithSkeleton: a pattern added without a skeleton of its own
            # (a style pattern, a canonical item) is dropped when its BASE skeleton is already present, so the
            # medium date pattern shadows the short one; availableFormats entries replace a same-skeleton one.
            specified = skel is not None
            if skel is None:
                skel = skeleton_of(pattern)
            key = Skeleton(skel)
            k = canonical_key(key)
            base = base_key(key)
            if not specified and base in bases:
                return
            if k in cands and not override:
                return
            bases.add(base)
            cands[k] = (key, pattern)
        for style in ('full', 'long', 'medium', 'short'):
            for block in ('dateFormats', 'timeFormats'):
                p = g[block][style]
                if isinstance(p, dict):
                    p = p.get('_value')
                add(p)
        for ch in 'GyQMwWEDFdaHmsSv':
            add(ch, ch)
        seen = set()
        for k, v in dtf['availableFormats'].items():
            if '-alt-' in k or '-count-' in k:
                continue
            ck = canonical_key(Skeleton(k))
            add(v, k, override=ck not in seen)
            seen.add(ck)
        self.cands = cands


def skeleton_of(pattern):
    return ''.join(tok[1] * tok[2] for tok in tokenize(pattern) if tok[0] == 'field')


def canonical_key(sk):
    return tuple((f, sk.orig[f]) for f in sorted(sk.orig))


def base_key(sk):
    """ICU's baseOriginal: each field at its canonical row's minimum length, so d and dd share a base."""
    out = []
    for f in sorted(sk.orig):
        ch, ln = sk.orig[f]
        row = canonical_row(ch, ln)
        out.append((f, row[0], row[3]))
    return tuple(out)


def get_best_raw(ld, req, include_mask):
    best = None
    # ICU iterates its PatternMap by the base skeleton's first letter, upper case first; ties keep the first.
    def order(item):
        sk = item[1][0]
        first = sk.orig[min(sk.orig)][0] if sk.orig else '~'
        return (0 if first.isupper() else 1, first.lower(), sk.text)
    for key, (sk, pattern) in sorted(ld.cands.items(), key=order):
        d, missing, extra = distance(req, include_mask, sk)
        if best is None or d < best[0]:
            best = (d, missing, extra, sk, pattern)
            if d == 0:
                break
    return best


def adjust_field_types(pattern, req, specified, fix_fractional=False, decimal='.'):
    out = []
    for tok in tokenize(pattern):
        if tok[0] == 'lit':
            out.append(tok[1])
            continue
        ch, ln = tok[1], tok[2]
        row = canonical_row(ch, ln)
        if row is None:
            out.append(ch * ln)
            continue
        f = row[1]
        if fix_fractional and f == SECOND:
            out.append(ch * ln + decimal + 'S' * req.orig[FRACSEC][1])
            continue
        if req.type[f] == 0:
            out.append(ch * ln)
            continue
        rc, rl = req.orig[f]
        if rc == 'E' and rl < 3:
            rl = 3
        adj = rl
        if f in (MINUTE, SECOND):
            adj = ln   # V8 passes UDATPG_MATCH_HOUR_FIELD_LENGTH only
        elif specified is not None and rc not in 'ce' and f in specified.orig:
            sl = specified.orig[f][1]
            pat_numeric = row[2] > 0
            skel_numeric = specified.type[f] > 0
            if sl == rl or pat_numeric != skel_numeric:
                adj = ln
        c = rc if (f not in (HOUR, MONTH, WEEKDAY) and (f != YEAR or rc == 'Y')) else ch
        out.append(c * adj)
    return ''.join(out)


def get_best_appending(ld, req, missing_fields, decimal):
    if missing_fields == 0:
        return ''
    d, missing, extra, sk, pattern = get_best_raw(ld, req, missing_fields)
    result = adjust_field_types(pattern, req, sk, decimal=decimal)
    if missing == 0 and extra == 0:
        return result
    frac = (1 << SECOND) | (1 << FRACSEC)
    if (missing & frac) == (1 << FRACSEC) and (missing_fields & frac) == frac:
        result = adjust_field_types(pattern, req, sk, fix_fractional=True, decimal=decimal)
        missing &= ~(1 << FRACSEC)
    guard = 0
    while missing:
        guard += 1
        if guard > FIELD_COUNT:
            break
        start = missing
        d2, missing2, extra2, sk2, pattern2 = get_best_raw(ld, req, missing)
        temp = adjust_field_types(pattern2, req, sk2, decimal=decimal)
        found = start & ~missing2
        if found == 0:
            break
        top = found.bit_length() - 1
        key = APPEND_KEYS.get(top)
        if key and key in ld.append_items:
            name = (ld.fields.get(FIELD_NAME_KEYS[top], {}) or {}).get('displayName', '')
            result = ld.append_items[key].replace('{0}', result).replace('{1}', temp).replace('{2}', "'" + name + "'")
        missing = missing2
    return result


def best_pattern(ld, skeleton_text, decimal='.'):
    req = Skeleton(skeleton_text)
    d, missing, extra, sk, pattern = get_best_raw(ld, req, -1)
    if missing == 0 and extra == 0:
        return adjust_field_types(pattern, req, sk, decimal=decimal)
    needed = req.mask()
    date = get_best_appending(ld, req, needed & DATE_MASK, decimal)
    time = get_best_appending(ld, req, needed & TIME_MASK, decimal)
    if not date:
        return time
    if not time:
        return date
    month_len = req.orig.get(MONTH, ('M', 0))[1]
    style = 'short'
    if month_len == 4:
        style = 'full' if WEEKDAY in req.orig else 'long'
    elif month_len == 3:
        style = 'medium'
    fmt = ld.dt_formats_at[style]
    # {1} is the date, {0} the time; the joining pattern's own literals are quoted already.
    return fmt.replace('{1}', '\u0001').replace('{0}', time).replace('\u0001', date)


# ---------------------------------------------------------------------------------------------------
# ECMA-402 BasicFormatMatcher over the same candidate list, for comparison.
TABLE16 = ['weekday', 'era', 'year', 'month', 'day', 'dayPeriod', 'hour', 'minute', 'second', 'fractionalSecondDigits', 'timeZoneName']


def record_of(pattern):
    rec = {}
    for tok in tokenize(pattern):
        if tok[0] != 'field':
            continue
        ch, ln = tok[1], tok[2]
        if ch in 'Eec':
            rec['weekday'] = 'narrow' if ln == 5 else 'long' if ln == 4 else 'short'
        elif ch == 'G':
            rec['era'] = 'narrow' if ln == 5 else 'long' if ln == 4 else 'short'
        elif ch in 'yY':
            rec['year'] = '2-digit' if ln == 2 else 'numeric'
        elif ch in 'ML':
            rec['month'] = {1: 'numeric', 2: '2-digit', 3: 'short', 4: 'long', 5: 'narrow'}[min(ln, 5)]
        elif ch == 'd':
            rec['day'] = '2-digit' if ln == 2 else 'numeric'
        elif ch == 'B':
            rec['dayPeriod'] = 'narrow' if ln == 5 else 'long' if ln == 4 else 'short'
        elif ch in 'hHkK':
            rec['hour'] = '2-digit' if ln == 2 else 'numeric'
        elif ch == 'm':
            rec['minute'] = '2-digit' if ln == 2 else 'numeric'
        elif ch == 's':
            rec['second'] = '2-digit' if ln == 2 else 'numeric'
        elif ch == 'S':
            rec['fractionalSecondDigits'] = ln
        elif ch in 'zvO':
            rec['timeZoneName'] = 'short'
    return rec


def basic_format_matcher(ld, options):
    values = ['2-digit', 'numeric', 'narrow', 'short', 'long']
    best = None
    for key, (sk, pattern) in ld.cands.items():
        rec = record_of(pattern)
        score = 0
        for prop in TABLE16:
            o = options.get(prop)
            f = rec.get(prop)
            if o is None and f is not None:
                score -= 20
            elif o is not None and f is None:
                score -= 120
            elif o is not None and o != f:
                if prop == 'fractionalSecondDigits' or prop == 'timeZoneName':
                    score -= 120
                    continue
                delta = max(min(values.index(f) - values.index(o), 2), -2)
                score -= {2: 6, 1: 3, -1: 6, -2: 8}[delta]
        if best is None or score > best[0]:
            best = (score, pattern)
    return best[1]


# ---------------------------------------------------------------------------------------------------
# Skeleton from an options bag, as V8 builds it; hour letter from the resolved hour cycle.
def skeleton_from_options(o, hc):
    s = ''
    w = {'narrow': 'EEEEE', 'short': 'EEE', 'long': 'EEEE'}
    if 'weekday' in o: s += w[o['weekday']]
    if 'era' in o: s += {'narrow': 'GGGGG', 'short': 'G', 'long': 'GGGG'}[o['era']]
    if 'year' in o: s += {'2-digit': 'yy', 'numeric': 'y'}[o['year']]
    if 'month' in o: s += {'2-digit': 'MM', 'numeric': 'M', 'narrow': 'MMMMM', 'short': 'MMM', 'long': 'MMMM'}[o['month']]
    if 'day' in o: s += {'2-digit': 'dd', 'numeric': 'd'}[o['day']]
    if 'dayPeriod' in o: s += {'narrow': 'BBBBB', 'short': 'B', 'long': 'BBBB'}[o['dayPeriod']]
    if 'hour' in o:
        ch = {'h11': 'K', 'h12': 'h', 'h23': 'H', 'h24': 'k'}[hc]
        s += ch * (2 if o['hour'] == '2-digit' else 1)
    if 'minute' in o: s += 'mm' if o['minute'] == '2-digit' else 'm'
    if 'second' in o: s += 'ss' if o['second'] == '2-digit' else 's'
    if 'fractionalSecondDigits' in o: s += 'S' * o['fractionalSecondDigits']
    if 'timeZoneName' in o: s += {'short': 'z', 'long': 'zzzz'}[o['timeZoneName']]
    return s


def replace_hour_cycle(pattern, hc):
    ch = {'h11': 'K', 'h12': 'h', 'h23': 'H', 'h24': 'k'}[hc]
    out = []
    for tok in tokenize(pattern):
        if tok[0] == 'field' and tok[1] in 'hHkK':
            out.append(ch * tok[2])
        elif tok[0] == 'field':
            out.append(tok[1] * tok[2])
        else:
            out.append(tok[1])
    return ''.join(out)


# ---------------------------------------------------------------------------------------------------
# Rendering (probe date only: 2022-12-24T15:07:09Z, a Saturday, UTC).
DATE = dict(year=2022, month=12, day=24, weekday='sat', hour=15, minute=7, second=9, ms=0)


def render(ld, pattern, hc):
    g = ld.g
    out = []
    for tok in tokenize(pattern):
        if tok[0] == 'lit':
            t = tok[1]
            if t.startswith("'"):
                t = t[1:-1] if len(t) >= 2 and t.endswith("'") else t[1:]
                t = t.replace("''", "'") if t else "'"
            out.append(t)
            continue
        ch, ln = tok[1], tok[2]
        if ch == 'G':
            w = 'eraNarrow' if ln == 5 else 'eraNames' if ln == 4 else 'eraAbbr'
            out.append(g['eras'][w]['1'])
        elif ch == 'y':
            out.append('%02d' % (DATE['year'] % 100) if ln == 2 else str(DATE['year']))
        elif ch in 'ML':
            ctx = 'format' if ch == 'M' else 'stand-alone'
            if ln <= 2:
                out.append(('%02d' if ln == 2 else '%d') % DATE['month'])
            else:
                w = {3: 'abbreviated', 4: 'wide'}.get(ln, 'narrow')
                out.append(g['months'][ctx][w][str(DATE['month'])])
        elif ch in 'Ec':
            ctx = 'format' if ch == 'E' else 'stand-alone'
            w = {4: 'wide', 5: 'narrow', 6: 'short'}.get(ln, 'abbreviated')
            out.append(g['days'][ctx][w][DATE['weekday']])
        elif ch == 'd':
            out.append(('%02d' if ln == 2 else '%d') % DATE['day'])
        elif ch in 'ab':
            w = {4: 'wide', 5: 'narrow'}.get(ln, 'abbreviated')
            out.append(g['dayPeriods']['format'][w]['pm' if DATE['hour'] >= 12 else 'am'])
        elif ch == 'B':
            out.append('<B>')
        elif ch in 'hHkK':
            h = DATE['hour']
            v = {'h': (h % 12) or 12, 'K': h % 12, 'H': h, 'k': h or 24}[ch]
            out.append(('%02d' if ln == 2 else '%d') % v)
        elif ch == 'm':
            out.append(('%02d' if ln == 2 else '%d') % DATE['minute'])
        elif ch == 's':
            out.append(('%02d' if ln == 2 else '%d') % DATE['second'])
        elif ch == 'S':
            out.append('0' * ln)
        else:
            out.append('<' + ch * ln + '>')
    # V8 writes a plain space where CLDR 42+ has U+202F in time patterns (format/formatToParts only).
    return ''.join(out).replace(' ', ' ')


# ---------------------------------------------------------------------------------------------------
BAGS = [
    ('E_d_MMMM', {'weekday': 'short', 'day': 'numeric', 'month': 'long'}),
    ('EEEE_d_MMMM', {'weekday': 'long', 'day': 'numeric', 'month': 'long'}),
    ('E_y_MMM_d', {'weekday': 'short', 'year': 'numeric', 'month': 'short', 'day': 'numeric'}),
    ('EEEE_y_MMMM_d', {'weekday': 'long', 'year': 'numeric', 'month': 'long', 'day': 'numeric'}),
    ('y_MMMM', {'year': 'numeric', 'month': 'long'}),
    ('y_MMM', {'year': 'numeric', 'month': 'short'}),
    ('y_MM', {'year': 'numeric', 'month': '2-digit'}),
    ('MMMM_d', {'month': 'long', 'day': 'numeric'}),
    ('MMM_d', {'month': 'short', 'day': 'numeric'}),
    ('M_d', {'month': 'numeric', 'day': 'numeric'}),
    ('y_M_d', {'year': 'numeric', 'month': 'numeric', 'day': 'numeric'}),
    ('yy_MM_dd', {'year': '2-digit', 'month': '2-digit', 'day': '2-digit'}),
    ('MMMM', {'month': 'long'}),
    ('EEEE', {'weekday': 'long'}),
    ('d', {'day': 'numeric'}),
    ('G_y', {'era': 'short', 'year': 'numeric'}),
    ('GGGG_y_MMMM_d', {'era': 'long', 'year': 'numeric', 'month': 'long', 'day': 'numeric'}),
    ('j_mm', {'hour': 'numeric', 'minute': '2-digit'}),
    ('H_mm_h23', {'hour': 'numeric', 'minute': '2-digit', 'hourCycle': 'h23'}),
    ('h_mm_h12', {'hour': 'numeric', 'minute': '2-digit', 'hour12': True}),
    ('j_mm_ss', {'hour': 'numeric', 'minute': 'numeric', 'second': 'numeric'}),
    ('E_j_mm', {'weekday': 'short', 'hour': 'numeric', 'minute': '2-digit'}),
    ('y_MMMM_d_j_mm', {'year': 'numeric', 'month': 'long', 'day': 'numeric', 'hour': 'numeric', 'minute': '2-digit'}),
    ('MMM_d_j_mm', {'month': 'short', 'day': 'numeric', 'hour': 'numeric', 'minute': '2-digit'}),
    ('y_M_d_j_mm_ss', {'year': 'numeric', 'month': 'numeric', 'day': 'numeric', 'hour': 'numeric', 'minute': 'numeric', 'second': 'numeric'}),
]

LIKELY_REGION = {'en': 'US', 'en-GB': 'GB', 'de': 'DE', 'fr': 'FR', 'es': 'ES', 'ru': 'RU', 'pl': 'PL', 'ja': 'JP', 'zh': 'CN',
                 'ko': 'KR', 'ar': 'EG'}
CYCLE = {'H': 'h23', 'h': 'h12', 'K': 'h11', 'k': 'h24'}


_likely = None


def likely_region(root, locale):
    global _likely
    parts = locale.split('-')
    for p in parts[1:]:
        if len(p) == 2 and p.isalpha() or (len(p) == 3 and p.isdigit()):
            return p.upper()
    if _likely is None:
        _likely = json.load(open(os.path.join(root, 'core', 'package', 'supplemental', 'likelySubtags.json'), encoding='utf-8'))['supplemental']['likelySubtags']
    full = _likely.get(locale) or _likely.get(parts[0]) or 'und-Latn-001'
    return full.split('-')[-1]


_time_data = None


def load_time_data(root):
    """timeData from supplementalData.xml when it is beside the JSON (cldr-json drops the language_region keys,
    fr_CA among them, which the XML and Jint's own TimeData table carry), the lossy JSON otherwise."""
    global _time_data
    if _time_data is not None:
        return _time_data
    xml = os.path.join(root, 'supplementalData.xml')
    if os.path.exists(xml):
        import xml.etree.ElementTree as ET
        _time_data = {}
        for hours in ET.parse(xml).getroot().iter('hours'):
            for region in hours.get('regions').split():
                _time_data[region] = {'_preferred': hours.get('preferred'), '_allowed': hours.get('allowed')}
    else:
        _time_data = json.load(open(os.path.join(root, 'core', 'package', 'supplemental', 'timeData.json'), encoding='utf-8'))['supplemental']['timeData']
    return _time_data


def hour_cycles(root, locale):
    td = load_time_data(root)
    region = likely_region(root, locale)
    lang = locale.split('-')[0]
    entry = td.get(lang + '_' + region) or td.get(region) or td['001']
    preferred = CYCLE[entry['_preferred'][0]]
    allowed = [CYCLE[a[0]] for a in entry['_allowed'].split()]
    h12 = next((c for c in allowed if c in ('h11', 'h12')), 'h12')
    h24 = next((c for c in allowed if c in ('h23', 'h24')), 'h23')
    return preferred, h12, h24


def main():
    root = sys.argv[1]
    node = {}
    for line in open(sys.argv[2], encoding='utf-8'):
        line = line.rstrip('\r\n')
        import re
        line = re.sub(r'\\u([0-9a-f]{4})', lambda m: chr(int(m.group(1), 16)), line)
        cols = line.split('\t')
        if len(cols) >= 3:
            node[(cols[0], cols[1])] = cols[2]
    basic = '--basic' in sys.argv
    same = diff = 0
    locales = []
    for loc, _ in node:
        if loc not in locales:
            locales.append(loc)
    for locale in locales:
        if not os.path.isdir(os.path.join(root, 'package', 'main', locale)):
            print('skip', locale, '(no cldr-json directory)')
            continue
        ld = LocaleData(root, locale)
        preferred, h12, h24 = hour_cycles(root, locale)
        for name, bag in BAGS:
            o = {k: v for k, v in bag.items() if k not in ('hourCycle', 'hour12')}
            hc = bag.get('hourCycle') or (h12 if bag.get('hour12') is True else h24 if bag.get('hour12') is False else preferred)
            if basic:
                pattern = basic_format_matcher(ld, o)
            else:
                pattern = best_pattern(ld, skeleton_from_options(o, hc))
            if 'hour' in o:
                pattern = replace_hour_cycle(pattern, hc)
            text = render(ld, pattern, hc)
            expected = node.get((locale, name))
            ok = text == expected
            same += ok
            diff += not ok
            print(('  ' if ok else '!!') + '\t%s\t%s\t%r\t%r\tnode=%r' % (locale, name, pattern, text, expected))
    print('same', same, 'diff', diff)


if __name__ == '__main__':
    main()
