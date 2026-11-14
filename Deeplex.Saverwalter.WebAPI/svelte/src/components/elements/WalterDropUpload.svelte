<!-- Copyright (C) 2023-2026  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->
<!--
    Dateien vom Rechner irgendwo auf die Seite ziehen lädt sie in die Ablage des Objekts hoch
    (in das Jahr des Objekts, sonst ohne Jahr). Der Upload-Bereich im Anhänge-Panel behandelt seine eigenen Drops
    selbst (Upload-Bereich, Listen nach Jahr) und werden hier ausgenommen.
-->
<script lang="ts">
    import { noYear, type WalterFileWrapper } from '$walter/lib';
    import { fileURL } from '$walter/services/files';
    import { upload_new_files } from '../subdetails/WalterAnhaenge';

    export let fileWrapper: WalterFileWrapper;

    // Zählt Betreten und Verlassen der Elemente, damit das Zeichen nicht flackert. Die Handler
    // laufen in der Capture-Phase: Der Upload-Bereich im Panel hält seine Ereignisse zurück.
    let depth = 0;

    // Nur Dateien vom Rechner; Dateien aus der Liste bringen eigene Daten mit.
    const isFiles = (event: DragEvent) =>
        !!event.dataTransfer?.types.includes('Files');

    // Der Upload-Bereich und die Listen im Panel nehmen ihre Drops selbst an.
    const inPanelDropArea = (event: DragEvent) =>
        event.target instanceof Element &&
        !!event.target.closest(
            '.bx--file__drop-container, [data-walter-dropzone]'
        );

    function enter(event: DragEvent) {
        if (isFiles(event)) depth++;
    }

    function leave(event: DragEvent) {
        if (isFiles(event)) depth = Math.max(0, depth - 1);
    }

    function over(event: DragEvent) {
        // Sonst öffnet der Browser die Datei.
        if (isFiles(event)) event.preventDefault();
    }

    async function drop(event: DragEvent) {
        if (!isFiles(event)) return;
        depth = 0;
        if (inPanelDropArea(event)) return;
        event.preventDefault();

        const files = [...(event.dataTransfer?.files ?? [])];
        const handle = fileWrapper.handles[0];
        if (files.length === 0 || !handle) return;

        await upload_new_files(
            handle,
            files,
            fileWrapper.fetchImpl,
            handle.fileURL === fileURL.stack
                ? undefined
                : (handle.defaultYear ?? noYear)
        );
        fileWrapper = fileWrapper;
    }
</script>

<svelte:window
    on:dragenter|capture={enter}
    on:dragleave|capture={leave}
    on:dragover|capture={over}
    on:drop|capture={drop}
/>

{#if depth > 0}
    <div class="overlay">Dateien hier ablegen, um sie hochzuladen</div>
{/if}

<style>
    .overlay {
        position: fixed;
        inset: 0.5rem;
        z-index: 9000;
        display: flex;
        align-items: center;
        justify-content: center;
        border: 2px dashed var(--cds-interactive, #0f62fe);
        background-color: rgba(15, 98, 254, 0.08);
        color: var(--cds-text-primary, #161616);
        font-size: 1.25rem;
        pointer-events: none;
    }
</style>
