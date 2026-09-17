import re

for path in ['/workspace/MolydeuxDemos/Common/MolyGame.cs']:
    s = open(path).read()
    # Strip string literals and comments (rough but good enough for brace counting)
    s = re.sub(r'/\*.*?\*/', '', s, flags=re.S)
    s = re.sub(r'//[^\n]*', '', s)
    s = re.sub(r'"(?:\\.|[^"\\])*"', '""', s)
    s = re.sub(r"'(?:\\.|[^'\\])*'", "''", s)
    print(f'{path}:')
    print(f'  {{ = {s.count("{")}, }} = {s.count("}")}, balanced = {s.count("{") == s.count("}")}')
    print(f'  ( = {s.count("(")}, ) = {s.count(")")}, balanced = {s.count("(") == s.count(")")}')

# Check all 188 demo Game.cs files
issues = 0
import os, glob
for path in sorted(glob.glob('/workspace/MolydeuxDemos/Demos/*/Game.cs')):
    s = open(path).read()
    s2 = re.sub(r'/\*.*?\*/', '', s, flags=re.S)
    s2 = re.sub(r'//[^\n]*', '', s2)
    s2 = re.sub(r'"(?:\\.|[^"\\])*"', '""', s2)
    if s2.count('{') != s2.count('}') or s2.count('(') != s2.count(')'):
        print(f'IMBALANCE: {path} {{={s2.count("{")}}} }}={s2.count("}")} (={s2.count("(")} )={s2.count(")")}')
        issues += 1
print(f'\n{issues} demo files with brace/paren imbalance')
