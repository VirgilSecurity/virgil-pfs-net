import msbuilder

proj = r'..\Virgil.PFS.NetFx\Virgil.PFS.NetFx.csproj'
output = r'.\output'

builder = msbuilder.MsBuilder()
builder.build(proj)
builder.pack(proj, output)