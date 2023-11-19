<script lang="ts">
    import { WalterTextArea } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import WalterTextInput from '../elements/WalterTextInput.svelte';
    import type { WalterUmlagetypEntry } from '$walter/lib';

    export let entry: Partial<WalterUmlagetypEntry> = {};
    export let readonly = false;
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
</script>

<Row>
    <WalterTextInput
        labelText="Bezeichnung"
        required
        bind:value={entry.bezeichnung}
    />
</Row>

<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
