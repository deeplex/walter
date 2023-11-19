<script lang="ts">
    import type { WalterS3File } from '$walter/types';
    import { TextInput } from 'carbon-components-svelte';

    export let file: WalterS3File;
    export let value: string;
</script>

<p>Aktueller Dateiname: {file.FileName}</p>
<TextInput
    labelText="Neuer Dateiname"
    placeholder="Neuen Dateinamen eingeben"
    bind:value
/>
