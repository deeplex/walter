<script lang="ts">
    import { Loading, Tile } from 'carbon-components-svelte';
    import type { WalterFile } from '$walter/types';

    export let file: WalterFile;
</script>

{#if file.type}
    <Tile light>
        Kann für die Datei: <strong>{file.fileName}</strong> keine Vorschau
        anzeigen.
        <br />Für den Dateityp:
        <strong>{file.type}</strong> ist keine Vorschau verfügbar.
    </Tile>
{:else}
    <Loading />
{/if}
