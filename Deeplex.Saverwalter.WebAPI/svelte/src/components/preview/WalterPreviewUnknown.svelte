<script lang="ts">
    import { InlineLoading, Loading, Tile } from 'carbon-components-svelte';
    import type { WalterS3File } from '$walter/types';

    export let file: WalterS3File;
</script>

{#if file.Type}
    <Tile light>
        Kann für die Datei: <strong>{file.FileName}</strong> keine Vorschau
        anzeigen.
        <br />Für den Dateityp:
        <strong>{file.Type}</strong> ist keine Vorschau verfügbar.
    </Tile>
{:else}
    <Loading />
{/if}
