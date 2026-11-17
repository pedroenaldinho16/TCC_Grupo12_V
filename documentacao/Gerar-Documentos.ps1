param([string]$BasePath = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
function EscapeXml([string]$s){return [Security.SecurityElement]::Escape($s)}
function Para([string]$text,[string]$style='Normal'){
 return '<w:p><w:pPr><w:pStyle w:val="'+$style+'"/></w:pPr><w:r><w:t xml:space="preserve">'+(EscapeXml $text)+'</w:t></w:r></w:p>'
}
function AddText($zip,[string]$name,[string]$text){
 $entry=$zip.CreateEntry($name);$stream=$entry.Open();$writer=[IO.StreamWriter]::new($stream,[Text.UTF8Encoding]::new($false));$writer.Write($text);$writer.Dispose()
}
$specs=Get-Content -Raw -Encoding UTF8 (Join-Path $PSScriptRoot 'conteudo-meta2.json') | ConvertFrom-Json
foreach($spec in $specs){
 $dest=Join-Path $PSScriptRoot $spec.filename
 if(Test-Path -LiteralPath $dest){$archiveDir=Join-Path $PSScriptRoot 'versoes-documentos';[IO.Directory]::CreateDirectory($archiveDir)|Out-Null;Move-Item -LiteralPath $dest -Destination (Join-Path $archiveDir ((Get-Date -Format yyyyMMdd-HHmmss)+'-'+$spec.filename))}
 $zip=[IO.Compression.ZipFile]::Open($dest,[IO.Compression.ZipArchiveMode]::Create)
 try{
  $body=[Text.StringBuilder]::new();$rels=[Text.StringBuilder]::new();$number=0;$figure=if($spec.filename -like '*Subsecao*'){15}else{0}
  [void]$rels.Append('<Relationship Id="rStyles" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>')
  foreach($block in $spec.blocks){
   if($block.kind -eq 'p'){[void]$body.Append((Para $block.text $block.style))}
   elseif($block.kind -eq 'palette'){
    [void]$body.Append('<w:tbl><w:tblPr><w:tblW w:w="9000" w:type="dxa"/><w:tblBorders><w:top w:val="single" w:sz="4" w:color="DCE8E2"/><w:bottom w:val="single" w:sz="4" w:color="DCE8E2"/><w:insideH w:val="single" w:sz="4" w:color="DCE8E2"/></w:tblBorders></w:tblPr>')
    foreach($row in $block.rows){
     [void]$body.Append('<w:tr><w:tc><w:tcPr><w:tcW w:w="800" w:type="dxa"/><w:shd w:fill="'+$row[0]+'"/></w:tcPr><w:p/></w:tc>')
     foreach($cell in @($row[1],$row[2],$row[3])){[void]$body.Append('<w:tc>'+ (Para $cell)+'</w:tc>')}
     [void]$body.Append('</w:tr>')
    };[void]$body.Append('</w:tbl>')
   }
   elseif($block.kind -eq 'image'){
    $source=Join-Path $PSScriptRoot $block.file
    if(!(Test-Path -LiteralPath $source)){throw "Captura ausente: $source"}
    $number++;$figure++;$bytes=[IO.File]::ReadAllBytes($source)
    $width=([int]$bytes[16] -shl 24) -bor ([int]$bytes[17] -shl 16) -bor ([int]$bytes[18] -shl 8) -bor [int]$bytes[19]
    $height=([int]$bytes[20] -shl 24) -bor ([int]$bytes[21] -shl 16) -bor ([int]$bytes[22] -shl 8) -bor [int]$bytes[23]
    $scale=[Math]::Min(5688000.0/$width,6500000.0/$height)
    if($block.file -like '*image1.png'){$scale=[Math]::Min($scale,1200000.0/$width)}
    $cx=[int64]($width*$scale);$cy=[int64]($height*$scale)
    $rid='img'+$number;$file='image'+$number+'.png'
    $entry=$zip.CreateEntry('word/media/'+$file);$s=$entry.Open();$s.Write($bytes,0,$bytes.Length);$s.Dispose()
    [void]$rels.Append('<Relationship Id="'+$rid+'" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="media/'+$file+'"/>')
    $caption=EscapeXml $block.caption
    [void]$body.Append((Para ('Figura '+$figure+' — '+$block.caption) 'Caption'))
    [void]$body.Append(@"
<w:p><w:pPr><w:jc w:val="center"/></w:pPr><w:r><w:drawing><wp:inline distT="0" distB="0" distL="0" distR="0"><wp:extent cx="$cx" cy="$cy"/><wp:docPr id="$number" name="$file" descr="$caption"/><a:graphic><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture"><pic:pic><pic:nvPicPr><pic:cNvPr id="$number" name="$file"/><pic:cNvPicPr/></pic:nvPicPr><pic:blipFill><a:blip r:embed="$rid"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill><pic:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="$cx" cy="$cy"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom></pic:spPr></pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p>
"@)
    if($block.file -like '*image1.png'){[void]$body.Append((Para 'Fonte: identidade visual original do Grupo 12 (2026).' 'Caption'))}else{[void]$body.Append((Para 'Fonte: aplicação EcoCanta desenvolvida pelo Grupo 12 (2026). Captura real em ambiente local.' 'Caption'))}
   }
  }
  $document='<?xml version="1.0" encoding="UTF-8" standalone="yes"?><w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture"><w:body>'+$body.ToString()+'<w:sectPr><w:pgSz w:w="11906" w:h="16838"/><w:pgMar w:top="1440" w:right="1134" w:bottom="1440" w:left="1701"/></w:sectPr></w:body></w:document>'
  AddText $zip 'word/document.xml' $document
  AddText $zip 'word/_rels/document.xml.rels' ('<?xml version="1.0" encoding="UTF-8"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">'+$rels.ToString()+'</Relationships>')
  AddText $zip '_rels/.rels' '<?xml version="1.0" encoding="UTF-8"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="r1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/></Relationships>'
  AddText $zip '[Content_Types].xml' '<?xml version="1.0" encoding="UTF-8"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Default Extension="png" ContentType="image/png"/><Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/><Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/></Types>'
  AddText $zip 'word/styles.xml' @'
<?xml version="1.0" encoding="UTF-8"?><w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:docDefaults><w:rPrDefault><w:rPr><w:rFonts w:ascii="Arial" w:hAnsi="Arial"/><w:sz w:val="24"/><w:lang w:val="pt-BR"/></w:rPr></w:rPrDefault><w:pPrDefault><w:pPr><w:spacing w:after="160" w:line="360" w:lineRule="auto"/><w:jc w:val="both"/></w:pPr></w:pPrDefault></w:docDefaults><w:style w:type="paragraph" w:default="1" w:styleId="Normal"><w:name w:val="Normal"/></w:style><w:style w:type="paragraph" w:styleId="Title"><w:name w:val="Title"/><w:pPr><w:keepNext/><w:spacing w:before="300" w:after="300"/><w:jc w:val="left"/></w:pPr><w:rPr><w:b/><w:sz w:val="32"/><w:color w:val="214F46"/></w:rPr></w:style><w:style w:type="paragraph" w:styleId="Subtitle"><w:name w:val="Subtitle"/><w:pPr><w:jc w:val="left"/></w:pPr><w:rPr><w:sz w:val="22"/><w:color w:val="536960"/></w:rPr></w:style><w:style w:type="paragraph" w:styleId="Heading1"><w:name w:val="heading 1"/><w:pPr><w:keepNext/><w:spacing w:before="400" w:after="180"/><w:outlineLvl w:val="0"/><w:jc w:val="left"/></w:pPr><w:rPr><w:b/><w:sz w:val="28"/></w:rPr></w:style><w:style w:type="paragraph" w:styleId="Heading2"><w:name w:val="heading 2"/><w:pPr><w:keepNext/><w:spacing w:before="280" w:after="160"/><w:outlineLvl w:val="1"/><w:jc w:val="left"/></w:pPr><w:rPr><w:b/><w:sz w:val="26"/></w:rPr></w:style><w:style w:type="paragraph" w:styleId="Heading3"><w:name w:val="heading 3"/><w:pPr><w:keepNext/><w:spacing w:before="300" w:after="160"/><w:outlineLvl w:val="2"/><w:jc w:val="left"/></w:pPr><w:rPr><w:b/><w:sz w:val="24"/></w:rPr></w:style><w:style w:type="paragraph" w:styleId="Caption"><w:name w:val="caption"/><w:pPr><w:spacing w:before="100" w:after="120" w:line="240"/><w:jc w:val="center"/></w:pPr><w:rPr><w:sz w:val="20"/></w:rPr></w:style></w:styles>
'@
 }finally{$zip.Dispose()}
 Write-Output ('DOCX criado: '+$dest)
 $verify=[IO.Compression.ZipFile]::OpenRead($dest)
 try{
  foreach($entry in $verify.Entries | Where-Object {$_.Name -like '*.xml' -or $_.Name -like '*.rels'}){$reader=[IO.StreamReader]::new($entry.Open());[xml]$validated=$reader.ReadToEnd();$reader.Dispose()}
  Write-Output ('Pacote validado: '+$verify.Entries.Count+' partes; '+$number+' imagens reais.')
 }finally{$verify.Dispose()}
}
