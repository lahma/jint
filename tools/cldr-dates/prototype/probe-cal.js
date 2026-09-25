// Premise probe for #4158, calendars: which patterns does a non-Gregorian calendar pick?
var out = typeof print === 'function' ? print : function (s) { console.log(s); };
function esc(s) { return s.replace(/[^\x20-\x7e]/g, function (c) { return String.fromCharCode(92) + 'u' + ('0000' + c.charCodeAt(0).toString(16)).slice(-4); }); }
var date = new Date(Date.UTC(2022, 11, 24, 15, 7, 9));
var locales = ['en', 'de', 'ja'];
var cals = ['gregory', 'buddhist', 'japanese', 'roc', 'hebrew', 'islamic-civil', 'persian', 'chinese', 'coptic'];
var bags = [
  ['default', {}],
  ['y_MMMM_d', { year: 'numeric', month: 'long', day: 'numeric' }],
  ['E_y_MMM_d', { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' }],
  ['y_MMM', { year: 'numeric', month: 'short' }],
];
for (var li = 0; li < locales.length; li++) {
  for (var ci = 0; ci < cals.length; ci++) {
    for (var bi = 0; bi < bags.length; bi++) {
      var loc = locales[li] + '-u-ca-' + cals[ci], name = cals[ci] + ':' + bags[bi][0];
      var line;
      try {
        var f = new Intl.DateTimeFormat(loc, Object.assign({ timeZone: 'UTC' }, bags[bi][1]));
        line = locales[li] + '\t' + name + '\t' + f.format(date) + '\t' + f.formatToParts(date).map(function (p) { return p.type === 'literal' ? JSON.stringify(p.value) : p.type; }).join(' ') + '\t';
      } catch (e) {
        line = locales[li] + '\t' + name + '\tERROR ' + e + '\t\t';
      }
      out(esc(line));
    }
  }
}
