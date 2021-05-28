import os
import string
from xml.sax.saxutils import escape

ref = os.environ['GITHUB_REF']
tag = ref.split('/')[2]
versionTemplate = string.Template("""<?xml version="1.0" encoding="UTF-8"?>
<item>
    <version>${version}</version>
    <url>https://github.com/sanslash332/tfj-sayclip/releases/latest/download/sayclip.zip</url>
    <changelog>https://github.com/sanslash332/tfj-sayclip/releases</changelog>
    <mandatory>false</mandatory>
</item>""")

xmlVersion = versionTemplate.substitute(version=escape(tag))
f = open('version.xml', 'w')
f.write(xmlVersion)
f.flush()
f.close()
print(f"Generated xml: {xmlVersion}")
