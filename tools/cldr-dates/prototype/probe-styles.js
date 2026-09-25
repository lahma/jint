// Premise probe for #4158, styles: dateStyle / timeStyle, which today read .NET patterns and a fixed time shape.
var out = typeof print === 'function' ? print : function (s) { console.log(s); };
function esc(s) { return s.replace(/[^\x20-\x7e]/g, function (c) { return String.fromCharCode(92) + 'u' + ('0000' + c.charCodeAt(0).toString(16)).slice(-4); }); }
var date = new Date(Date.UTC(2022, 11, 24, 15, 7, 9));
var locales = ['en', 'en-GB', 'de', 'fr', 'es', 'ru', 'pl', 'ja', 'zh', 'ko', 'ar'];
var styles = ['full', 'long', 'medium', 'short'];
for (var li = 0; li < locales.length; li++) {
  for (var si = 0; si < styles.length; si++) {
    var cases = [['date_' + styles[si], { dateStyle: styles[si] }], ['time_' + styles[si], { timeStyle: styles[si] }],
                 ['both_' + styles[si], { dateStyle: styles[si], timeStyle: 'short' }]];
    for (var ci = 0; ci < cases.length; ci++) {
      var line;
      try {
        line = locales[li] + '\t' + cases[ci][0] + '\t' + new Intl.DateTimeFormat(locales[li], Object.assign({ timeZone: 'UTC' }, cases[ci][1])).format(date) + '\t\t';
      } catch (e) {
        line = locales[li] + '\t' + cases[ci][0] + '\tERROR ' + e + '\t\t';
      }
      out(esc(line));
    }
  }
}
