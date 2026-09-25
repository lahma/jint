# Design prototype for #4158 (not shipped)

Scratch material for [#4158](https://github.com/sebastienros/jint/issues/4158): resolving an
`Intl.DateTimeFormat` component bag through a matched CLDR `availableFormats` pattern. Nothing here is built,
packed or run by CI; the implementation PRs replace it with a real generator under `tools/cldr-dates/` and
C# under `Jint/Native/Intl/`.

| File | What it is |
| --- | --- |
| `probe.js`, `probe-cal.js`, `probe-styles.js` | Option bags x locales, printed as TSV. Run under Node for the ICU reference and under `Jint.Repl` for Jint's current answer. `PROBE_LOCALES=a,b,c` overrides `probe.js`'s locale list (Node only). |
| `compare.py` | Diffs two probe TSVs (`fmt`, `ro` for resolvedOptions, `parts` for part types). |
| `format_matcher.py` | A Python model of ICU's `DateTimePatternGenerator` as V8 drives it, over CLDR 48.2's resolved JSON. |
| `size_estimate.py` | What embedding the data costs, per data group and locale set. |
| `baseline/` | Outputs recorded 2026-09-25: `jint*.tsv` from `main` at `7d9cdcc8f`, `node*.tsv` from Node 24.19.0 (ICU 78.3, CLDR 48.0). |

## Inputs

`format_matcher.py` and `size_estimate.py` read cldr-json 48.2.0 — the npm tarballs
`cldr-dates-full-48.2.0.tgz` (unpacked as `package/`) and `cldr-core-48.2.0.tgz` (unpacked as
`core/package/`) — plus CLDR 48.2's `common/supplemental/supplementalData.xml` copied beside them, because
cldr-json's `timeData.json` drops the `language_region` keys (`fr_CA`, `en_001`, ...) that the XML and Jint's
own `TimeData` table carry.

```sh
node probe.js > node.tsv
dotnet run --project Jint.Repl -c Release -- -f tools/cldr-dates/prototype/probe.js -t 60 > jint.tsv
python compare.py fmt jint.tsv node.tsv
python format_matcher.py <cldr-json-root> node.tsv            # the ICU-style best-fit model
python format_matcher.py <cldr-json-root> node.tsv --basic    # ECMA-402 BasicFormatMatcher, for contrast
python size_estimate.py <cldr-json-root>
```

## Results at the time of writing

- Jint against Node, 11 locales x 25 bags: 131 of 275 identical (`baseline/jint.tsv` vs `baseline/node.tsv`).
- `format_matcher.py` against Node, the same 275: 275 identical.
- `format_matcher.py` against Node, 52 locales x 25 bags: 1215 of 1275 identical. Of the 60 that differ, 59 are
  `bn`, `fa` and `th`, whose default numbering system or calendar the prototype does not model (it renders Latin
  digits and the Gregorian calendar), and one is `en-AU` `{ weekday: 'short', day: 'numeric', month: 'long' }`,
  where ICU writes `Sat 24 December` and the prototype `Sat, 24 December` from CLDR 48.2's resolved data; not
  explained.
- `--basic` over the raw `availableFormats` list: 142 of 275, which is why the design uses the best-fit model
  for both `formatMatcher` values.
- Size (`baseline/size-estimate.txt`): the data the format/formatToParts PR needs (availableFormats,
  dateTimeFormats with the atTime variants, appendItems, month/weekday names in both contexts, eras, am/pm) is
  713 KB of UTF-8 for all 766 cldr-json locales when each locale stores only what differs from its CLDR parent;
  96 KB deflated as one blob, 212 KB as one deflated block per language. Adding interval formats, style patterns
  and flexible day periods takes it to 1.55 MB / 158 KB / 378 KB.
