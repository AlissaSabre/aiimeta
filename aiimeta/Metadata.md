# Metadata location and format

List of metadata aiimeta can handle (for the moment).

## A1111

* PNG
  - "Parameters" text chunk: A1111 Parameter string.

* JPEG
  - EXIF UserComment (0x9286): A1111 Parameter string.

## ComfyUI core

* PNG
  - "Workflow" text chunk: Workflow JSON.
  - "Prompt" text chunk: Prompt JSON.

## ComfyUI custom_nodes variations

* PNG
  - "Parameters" text chunk: simulated A1111 Parameter string + some extension.
  - "Workflow" text chunk: Workflow JSON.
  - "Prompt" text chunk: Prompt JSON.

* WebP and JPEG
  - EXIF UserComment (0x9286): simulated A1111 Parameter string + some extension.
  - EXIF Image Description: "Workflow:" followed by Workflow JSON.
  - EXIF Make (0x010F): "workflow:" followed by Workflow JSON.
  - EXIF CameraModelName (0x0110): "prompt:" followed by Prompt JSON.

## Civitai

* JPEG
  - EXIF UserComment (0x9286): A1111 Parameter string + Civitai extension.
  - EXIF UserComment (0x9286): ComfyUI-style Workflow JSON + Civitai extension.

## SwarmUI

* PNG
  - "Parameters" text chunk: SwarmUI Image Metadata JSON.

* JPEG
  - EXIF UserComment (0x9286): SwarmUI Image Metadata JSON.

