$ErrorActionPreference='Stop'
$docsRoot=Split-Path -Parent $PSScriptRoot
$exportarPdf=$env:ECOCANTA_EXPORTAR_PDF -eq '1'
$result=@()
$word=$null
try {
    $word=New-Object -ComObject Word.Application
    $word.Visible=$false
    $word.DisplayAlerts=0
    $word.AutomationSecurity=3
    foreach($name in @('Meta2_Subsecao_4_2_EcoCanta','Meta2_Identidade_Visual_EcoCanta')){
        $path=Join-Path $docsRoot ($name+'.docx')
        $pdf=Join-Path $docsRoot ($name+'.pdf')
        $document=$null
        try {
            $document=$word.Documents.Open($path,$false,$true)
            $document.Repaginate()
            $pages=$document.ComputeStatistics(2)
            $images=$document.InlineShapes.Count
            $metadata=[pscustomobject]@{arquivo=$name+'.docx';abriuNoWord=$true;paginas=$pages;imagens=$images;pdfAtualizado=$false}
            if($exportarPdf){
                if(Test-Path -LiteralPath $pdf){Move-Item -LiteralPath $pdf -Destination (Join-Path $docsRoot ('versoes-documentos/'+(Get-Date -Format yyyyMMdd-HHmmss)+'-'+$name+'.pdf'))}
                $document.ExportAsFixedFormat($pdf,17)
                $metadata.pdfAtualizado=$true
            }
            $result+=$metadata
            $result|ConvertTo-Json|Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'documentos-word.json')
        } finally {if($document){$document.Close(0);[void][Runtime.InteropServices.Marshal]::ReleaseComObject($document)}}
    }
} finally {if($word){$word.Quit();[void][Runtime.InteropServices.Marshal]::ReleaseComObject($word)}}
$result|ConvertTo-Json|Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'documentos-word.json')
$result|Format-Table
