# Viewer States

显示模式显式建模：`FitWindow`、`FitWidth`、`FitHeight`、`Fill`、`ActualSize`、`ManualZoom`。

Transform State：Zoom、Pan X/Y、Rotation、Flip H/V。

Content State：Current Image、Directory Context、Current Index、Animation、Metadata、Color Profile。

Request State：Current Request ID、Decode State、Present State、Preload Queue、Cache State。

旧请求完成时，如果 Request ID 已过期，不得覆盖当前图片。
