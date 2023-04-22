<script lang="ts">
  import { Tile } from 'carbon-components-svelte';
  import type { WalterS3File } from '$WalterTypes';

  export let file: WalterS3File;
</script>

<Tile light>
  Kann für die Datei: <strong>{file.FileName}</strong> keine Vorschau anzeigen.
  <br />Datei konnte nicht gelesen werden.
</Tile>
