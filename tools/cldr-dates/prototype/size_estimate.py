"""Size estimate for #4158 with the compact encoding a generator would actually write.

One block per locale, holding only what differs from the resolved parent locale (CLDR parentLocales, then
truncation; single-subtag locales against an explicit 'root' block built from the XML root when given, else
against nothing). Name arrays are one line each ('|'-joined), patterns one 'skeleton=pattern' line each.
Prints raw UTF-8 and deflate-compressed sizes per data group and locale set.

  python size_estimate.py <cldr-json-root>
"""
import io
import json
import os
import sys
import zlib

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
ROOT = sys.argv[1]
MAIN = os.path.join(ROOT, 'package', 'main')
CORE = os.path.join(ROOT, 'core', 'package')
parents = json.load(open(os.path.join(CORE, 'supplemental', 'parentLocales.json'), encoding='utf-8'))['supplemental']['parentLocales']['parentLocale']
coverage = json.load(open(os.path.join(CORE, 'coverageLevels.json'), encoding='utf-8'))['effectiveCoverageLevels']
SKIP = ('MMMMW', 'yw', 'yQ', 'Q')
MONTHS = [str(i) for i in range(1, 13)]
DAYS = ['sun', 'mon', 'tue', 'wed', 'thu', 'fri', 'sat']


def parent_of(loc):
    if loc in parents:
        return parents[loc]
    if '-' in loc:
        return loc.rsplit('-', 1)[0]
    return None


def lines_for(loc, groups):
    p = os.path.join(MAIN, loc, 'ca-gregorian.json')
    g = json.load(open(p, encoding='utf-8'))['main'][loc]['dates']['calendars']['gregorian']
    dtf = g['dateTimeFormats']
    out = {}
    if 'af' in groups:
        for k, v in dtf['availableFormats'].items():
            if k.startswith(SKIP) or '-count-' in k or '-alt-' in k:
                continue
            out['a:' + k] = v
    if 'dt' in groups:
        at = g.get('dateTimeFormats-atTime', {}).get('standard', {})
        out['j:'] = '|'.join(dtf[k] for k in ('full', 'long', 'medium', 'short'))
        out['J:'] = '|'.join(at.get(k, dtf[k]) for k in ('full', 'long', 'medium', 'short'))
        out['p:'] = '|'.join(dtf['appendItems'][k] for k in sorted(dtf['appendItems']))
    if 'iv' in groups:
        for k, v in dtf['intervalFormats'].items():
            if isinstance(v, str):
                out['i:' + k] = v
                continue
            if k.startswith(SKIP) or '-alt-' in k:
                continue
            for d, pat in v.items():
                if '-alt-' not in d:
                    out['i:' + k + '/' + d] = pat
    if 'wdfmt' in groups:   # the minimum the issue names: format-context weekday names
        for w in ('abbreviated', 'wide', 'narrow', 'short'):
            out['E:' + w] = '|'.join(g['days']['format'][w][d] for d in DAYS)
    if 'names' in groups:
        for ctx, c in (('format', 'M'), ('stand-alone', 'L')):
            for w in ('abbreviated', 'wide', 'narrow'):
                out[c + ':' + w] = '|'.join(g['months'][ctx][w][m] for m in MONTHS)
        for ctx, c in (('format', 'E'), ('stand-alone', 'c')):
            for w in ('abbreviated', 'wide', 'narrow', 'short'):
                out[c + ':' + w] = '|'.join(g['days'][ctx][w][d] for d in DAYS)
        for w in ('eraAbbr', 'eraNames', 'eraNarrow'):
            out['G:' + w] = '|'.join(g['eras'][w][i] for i in ('0', '1'))
        for w in ('abbreviated', 'wide', 'narrow'):
            out['a:' + w] = '|'.join(g['dayPeriods']['format'][w][i] for i in ('am', 'pm'))
    if 'erasam' in groups:  # eras and am/pm only, for the option that leaves month/weekday names to .NET
        for w in ('eraAbbr', 'eraNames', 'eraNarrow'):
            out['G:' + w] = '|'.join(g['eras'][w][i] for i in ('0', '1'))
        for w in ('abbreviated', 'wide', 'narrow'):
            out['a:' + w] = '|'.join(g['dayPeriods']['format'][w][i] for i in ('am', 'pm'))
    if 'flex' in groups:    # flexible day periods, for the dayPeriod option (B)
        for w in ('abbreviated', 'wide', 'narrow'):
            dp = g['dayPeriods']['format'][w]
            out['B:' + w] = '|'.join(k + '=' + v for k, v in sorted(dp.items()) if k not in ('am', 'pm') and '-alt-' not in k)
    if 'styles' in groups:
        def val(x):
            return x if isinstance(x, str) else x.get('_value')
        out['d:'] = '|'.join(val(g['dateFormats'][k]) for k in ('full', 'long', 'medium', 'short'))
        out['t:'] = '|'.join(val(g['timeFormats'][k]) for k in ('full', 'long', 'medium', 'short'))
    return out


def estimate(locales, groups):
    cache = {}

    def get(loc):
        if loc not in cache:
            cache[loc] = lines_for(loc, groups) if os.path.isdir(os.path.join(MAIN, loc)) else None
        return cache[loc]

    text = []
    for loc in locales:
        data = get(loc)
        if data is None:
            continue
        par = parent_of(loc)
        pdata = get(par) if par else None
        pdata = pdata or {}
        diff = [(k, v) for k, v in sorted(data.items()) if pdata.get(k) != v]
        text.append('[' + loc + ']')
        text.extend(k + '=' + v for k, v in diff)
    blob = '\n'.join(text).encode('utf-8')
    return len(blob), len(zlib.compress(blob, 9))


all_locales = sorted(d for d in os.listdir(MAIN) if os.path.isdir(os.path.join(MAIN, d)))
modern = [l for l in all_locales if coverage.get(l) == 'modern']
top = [l for l in ['ar', 'bg', 'bn', 'ca', 'cs', 'da', 'de', 'el', 'en', 'en-GB', 'en-001', 'es', 'es-419', 'et', 'fa', 'fi',
                   'fil', 'fr', 'fr-CA', 'he', 'hi', 'hr', 'hu', 'id', 'it', 'ja', 'ko', 'lt', 'lv', 'ms', 'nb', 'nl', 'pl', 'pt',
                   'pt-PT', 'ro', 'ru', 'sk', 'sl', 'sr', 'sv', 'th', 'tr', 'uk', 'vi', 'zh', 'zh-Hant'] if l in all_locales]

print('%-44s %22s %22s %22s' % ('group', 'all (%d)' % len(all_locales), 'modern (%d)' % len(modern), 'top (%d)' % len(top)))
for label, groups in [
        ('availableFormats', {'af'}),
        ('dateTimeFormats(+atTime)+appendItems', {'dt'}),
        ('format-context weekday names only', {'wdfmt'}),
        ('all names (M/L, E/c, eras, am/pm)', {'names'}),
        ('flexible day periods (dayPeriod option)', {'flex'}),
        ('intervalFormats (formatRange)', {'iv'}),
        ('date/time style patterns', {'styles'}),
        ('PR-1 set: af + dt + all names', {'af', 'dt', 'names'}),
        ('lean: af + dt + fmt weekdays + eras/am-pm', {'af', 'dt', 'wdfmt', 'erasam'}),
        ('everything above', {'af', 'dt', 'names', 'flex', 'iv', 'styles'})]:
    cells = []
    for locs in (all_locales, modern, top):
        raw, z = estimate(locs, groups)
        cells.append('%8.1f KB / %6.1f KB' % (raw / 1024, z / 1024))
    print('%-44s %s' % (label, '  '.join(cells)))
print('(raw UTF-8 / deflate -9; each locale stores only what differs from its CLDR parent)')


def per_block(locales, groups):
    """Each locale block deflated on its own (random access to one locale chain without inflating the rest)."""
    total = 0
    raw_total = 0
    cache = {}
    for loc in locales:
        if loc not in cache:
            cache[loc] = lines_for(loc, groups)
        par = parent_of(loc)
        if par and par not in cache and os.path.isdir(os.path.join(MAIN, par)):
            cache[par] = lines_for(par, groups)
        pdata = cache.get(par) or {}
        block = '\n'.join(k + '=' + v for k, v in sorted(cache[loc].items()) if pdata.get(k) != v).encode('utf-8')
        raw_total += len(block)
        total += len(zlib.compress(block, 9)) + 8   # + an index entry
    return raw_total, total


for label, groups in [('PR-1 set: af + dt + all names', {'af', 'dt', 'names'}), ('everything', {'af', 'dt', 'names', 'flex', 'iv', 'styles'})]:
    raw, z = per_block(all_locales, groups)
    print('per-locale deflate, all locales, %-32s raw %8.1f KB -> %7.1f KB' % (label, raw / 1024, z / 1024))


def per_language(locales, groups):
    """One deflated block per language (all its regional variants together): what a lookup inflates."""
    by_lang = {}
    cache = {}
    for loc in locales:
        cache[loc] = lines_for(loc, groups)
    for loc in locales:
        par = parent_of(loc)
        pdata = cache.get(par) or {}
        block = '[' + loc + ']\n' + '\n'.join(k + '=' + v for k, v in sorted(cache[loc].items()) if pdata.get(k) != v)
        by_lang.setdefault(loc.split('-')[0], []).append(block)
    raw = z = 0
    biggest = (0, '')
    for lang, blocks in by_lang.items():
        b = '\n'.join(blocks).encode('utf-8')
        raw += len(b)
        c = len(zlib.compress(b, 9)) + 8
        z += c
        if len(b) > biggest[0]:
            biggest = (len(b), lang)
    return raw, z, len(by_lang), biggest


for label, groups in [('PR-1 set: af + dt + all names', {'af', 'dt', 'names'}), ('everything', {'af', 'dt', 'names', 'flex', 'iv', 'styles'})]:
    raw, z, n, big = per_language(all_locales, groups)
    print('per-language deflate (%d blocks), %-30s raw %8.1f KB -> %7.1f KB; largest block %s %.1f KB raw' % (n, label, raw / 1024, z / 1024, big[1], big[0] / 1024))
