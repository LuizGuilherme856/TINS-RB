# Shakesprite 0.1.1

Shakesprite is a dockable pixel editor for Unity 6.3. It runs in the Editor and uses Unity's built-in APIs. The standard PNG workflow needs no external apps or Unity packages.

## Quick start

1. Open **Window → 2D → Shakesprite**. Drag the tab next to Scene or Inspector to dock it.
2. Drag a PNG, a Sprite, or a sprite AnimationClip from **Project** into the asset field. You can also select the asset and click **Use Selection**.
3. For an unsliced spritesheet, choose **Grid** and set its columns and rows. Grid playback starts at the top left and proceeds left to right, then down. **Imported frames** uses existing Sprite slices. **Whole image** lets you edit the entire PNG.
4. Click **Play** to preview in a loop. Use the arrows or slider to select a frame. Set **FPS** for images without an AnimationClip.
5. Pick **Pencil** and a color, or use **Eraser** to make pixels transparent. Drag to draw a continuous line. The preview pauses as soon as you draw. Use **Undo** and **Redo** to step through strokes.
6. Click **Save PNG** to update and reimport the original PNG in place. Unity keeps its asset GUID, slices and import settings. **Save Copy** writes a separate PNG under Assets.

Choose **Fit** to see the full frame, or use a fixed zoom and middle-drag to pan. Each square of the transparency checkerboard matches one sprite pixel.

## Animation and file support

- PNG images up to 4096 × 4096 pixels and up to 4096 frames. Edit a source PNG at its original resolution even when its Unity import uses a smaller size or texture compression.
- For imported PNG slices, playback sorts by top edge, then left edge. Use an AnimationClip to control order and timing. A clip must use one sprite track from a single source sheet.
- The optional **2D Aseprite Importer** provided by Unity enables opening imported **.ase** and **.aseprite** files and their sprite clips. When the asset contains multiple clips, drag the desired clip from its expanded entry in Project. The editable image is the composited atlas that Unity imported.
- **Save PNG As** exports the edited Aseprite atlas as a new PNG. The original Aseprite document is untouched. Unity imports single-image copies as a Sprite with Point filtering. Multi-frame copies are unsliced PNGs. A copy carries the edited pixels, while frame layout and playback settings belong to the current Unity editing session.
- If the original PNG was modified outside Shakesprite, **Save PNG** prompts through the status bar to use **Save Copy**, preserving both versions.

## Primeiros passos em português

Abra **Window → 2D → Shakesprite**, selecione um PNG no painel **Project** e clique em **Use Selection**. Para uma spritesheet sem recortes, escolha **Grid** e informe as colunas e linhas. Use **Play** para ver a animação, **Pencil** para desenhar e **Eraser** para apagar. **Save PNG** grava no arquivo do projeto. **Save Copy** cria outro PNG dentro de Assets.
