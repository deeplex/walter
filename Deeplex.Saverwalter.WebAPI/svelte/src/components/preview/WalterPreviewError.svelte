<script lang="ts">
    import { Tile } from 'carbon-components-svelte';
    import type { WalterFile } from '$walter/types';

    export let file: WalterFile;
</script>

<Tile light>
    Kann für die Datei: <strong>{file.fileName}</strong> keine Vorschau
    anzeigen.
    <br />Datei konnte nicht gelesen werden.
</Tile>
