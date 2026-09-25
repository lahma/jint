// Premise probe for #4158: component bags, per locale, format() and the part types.
var out = typeof print === 'function' ? print : function (s) { console.log(s); };
function esc(s) { return s.replace(/[^\x20-\x7e]/g, function (c) { return String.fromCharCode(92) + 'u' + ('0000' + c.charCodeAt(0).toString(16)).slice(-4); }); }
var date = new Date(Date.UTC(2022, 11, 24, 15, 7, 9));
var locales = (typeof process !== 'undefined' && process.env && process.env.PROBE_LOCALES)
  ? process.env.PROBE_LOCALES.split(',')
  : ['en', 'en-GB', 'de', 'fr', 'es', 'ru', 'pl', 'ja', 'zh', 'ko', 'ar'];
var bags = [
  ['E_d_MMMM', { weekday: 'short', day: 'numeric', month: 'long' }],
  ['EEEE_d_MMMM', { weekday: 'long', day: 'numeric', month: 'long' }],
  ['E_y_MMM_d', { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' }],
  ['EEEE_y_MMMM_d', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }],
  ['y_MMMM', { year: 'numeric', month: 'long' }],
  ['y_MMM', { year: 'numeric', month: 'short' }],
  ['y_MM', { year: 'numeric', month: '2-digit' }],
  ['MMMM_d', { month: 'long', day: 'numeric' }],
  ['MMM_d', { month: 'short', day: 'numeric' }],
  ['M_d', { month: 'numeric', day: 'numeric' }],
  ['y_M_d', { year: 'numeric', month: 'numeric', day: 'numeric' }],
  ['yy_MM_dd', { year: '2-digit', month: '2-digit', day: '2-digit' }],
  ['MMMM', { month: 'long' }],
  ['EEEE', { weekday: 'long' }],
  ['d', { day: 'numeric' }],
  ['G_y', { era: 'short', year: 'numeric' }],
  ['GGGG_y_MMMM_d', { era: 'long', year: 'numeric', month: 'long', day: 'numeric' }],
  ['j_mm', { hour: 'numeric', minute: '2-digit' }],
  ['H_mm_h23', { hour: 'numeric', minute: '2-digit', hourCycle: 'h23' }],
  ['h_mm_h12', { hour: 'numeric', minute: '2-digit', hour12: true }],
  ['j_mm_ss', { hour: 'numeric', minute: 'numeric', second: 'numeric' }],
  ['E_j_mm', { weekday: 'short', hour: 'numeric', minute: '2-digit' }],
  ['y_MMMM_d_j_mm', { year: 'numeric', month: 'long', day: 'numeric', hour: 'numeric', minute: '2-digit' }],
  ['MMM_d_j_mm', { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' }],
  ['y_M_d_j_mm_ss', { year: 'numeric', month: 'numeric', day: 'numeric', hour: 'numeric', minute: 'numeric', second: 'numeric' }],
];
for (var li = 0; li < locales.length; li++) {
  for (var bi = 0; bi < bags.length; bi++) {
    var loc = locales[li], name = bags[bi][0], opts = Object.assign({ timeZone: 'UTC' }, bags[bi][1]);
    var line;
    try {
      var f = new Intl.DateTimeFormat(loc, opts);
      var s = f.format(date);
      var parts = f.formatToParts(date).map(function (p) { return p.type === 'literal' ? JSON.stringify(p.value) : p.type; }).join(' ');
      var ro = f.resolvedOptions();
      var roKeys = ['weekday', 'era', 'year', 'month', 'day', 'hour', 'minute', 'second', 'hourCycle'].filter(function (k) { return ro[k] !== undefined; }).map(function (k) { return k + '=' + ro[k]; }).join(',');
      line = loc + '\t' + name + '\t' + s + '\t' + parts + '\t' + roKeys;
    } catch (e) {
      line = loc + '\t' + name + '\tERROR ' + e;
    }
    out(esc(line));
  }
}
// formatRange samples
var ranges = [
  ['range_MMM_d_sameMonth', { month: 'short', day: 'numeric' }, Date.UTC(2022, 11, 24), Date.UTC(2022, 11, 27)],
  ['range_y_MMM_d_diffMonth', { year: 'numeric', month: 'short', day: 'numeric' }, Date.UTC(2022, 10, 24), Date.UTC(2022, 11, 3)],
  ['range_j_mm_sameDay', { hour: 'numeric', minute: '2-digit' }, Date.UTC(2022, 11, 24, 9, 0), Date.UTC(2022, 11, 24, 15, 30)],
  ['range_E_d_MMMM', { weekday: 'short', day: 'numeric', month: 'long' }, Date.UTC(2022, 11, 24), Date.UTC(2022, 11, 27)],
];
for (var li = 0; li < locales.length; li++) {
  for (var ri = 0; ri < ranges.length; ri++) {
    var r = ranges[ri];
    var line;
    try {
      var f = new Intl.DateTimeFormat(locales[li], Object.assign({ timeZone: 'UTC' }, r[1]));
      line = locales[li] + '\t' + r[0] + '\t' + f.formatRange(new Date(r[2]), new Date(r[3])) + '\t\t';
    } catch (e) {
      line = locales[li] + '\t' + r[0] + '\tERROR ' + e + '\t\t';
    }
    out(esc(line));
  }
}
