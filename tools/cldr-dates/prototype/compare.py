import re, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
UESC = re.compile(r'\\u([0-9a-f]{4})')

def load(p):
    rows = {}
    for line in open(p, encoding='utf-8'):
        line = line.rstrip('\r\n')
        line = UESC.sub(lambda m: chr(int(m.group(1), 16)), line)
        cols = line.split('\t')
        if len(cols) < 3:
            continue
        rows[(cols[0], cols[1])] = cols[2:]
    return rows

a = sys.argv[2] if len(sys.argv) > 2 else 'jint.tsv'
b = sys.argv[3] if len(sys.argv) > 3 else 'node.tsv'
j = load(a); n = load(b)
same = diff = 0
mode = sys.argv[1] if len(sys.argv) > 1 else 'fmt'
for k in n:
    nv = n[k]; jv = j.get(k, ['<missing>', '', ''])
    if mode == 'fmt':
        ok = nv[0] == jv[0]
        if ok: same += 1
        else: diff += 1
        print(('  ' if ok else '!!') + '\t' + k[0] + '\t' + k[1] + '\tJ: ' + repr(jv[0]) + '\tN: ' + repr(nv[0]))
    elif mode == 'ro':
        if len(nv) > 2 and len(jv) > 2 and nv[2] != jv[2]:
            print(k[0] + '\t' + k[1] + '\tJ: ' + jv[2] + '\tN: ' + nv[2])
    elif mode == 'parts':
        if len(nv) > 1 and len(jv) > 1 and nv[1] != jv[1]:
            print(k[0] + '\t' + k[1] + '\tJ: ' + jv[1] + '\n\t\t\tN: ' + nv[1])
if mode == 'fmt':
    print('same', same, 'diff', diff)
