<script lang="ts">
    import type { WalterFile } from '$walter/types';
    import { TextInput } from 'carbon-components-svelte';

    export let file: WalterFile;
    export let value: string;
</script>

<p>Aktueller Dateiname: {file.fileName}</p>
<TextInput
    labelText="Neuer Dateiname"
    placeholder="Neuen Dateinamen eingeben"
    bind:value
/>
