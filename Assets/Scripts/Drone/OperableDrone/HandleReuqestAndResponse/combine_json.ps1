# 定义输出文件名
$outputFile = "combined.json"

# 获取当前目录下的所有 JSON 文件
$jsonFiles = Get-ChildItem -Path . -Filter *.json

# 初始化一个空字符串用于存储合并后的内容
$combinedContent = "["

# 遍历所有 JSON 文件并读取它们的内容
foreach ($file in $jsonFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    $combinedContent += $content + "`n"  # 添加文件内容和换行符
}

$combinedContent += "]"  # 添加 JSON 数组的结束标记

# 将合并后的内容写入输出文件
Set-Content -Path $outputFile -Value $combinedContent

Write-Output "合并完成：$outputFile"
