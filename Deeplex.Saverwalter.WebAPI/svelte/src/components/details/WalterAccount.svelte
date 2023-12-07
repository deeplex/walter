<script lang="ts">
    import { Row } from 'carbon-components-svelte';
    import WalterTextInput from '../elements/WalterTextInput.svelte';
    import WalterMultiSelect from '../elements/WalterMultiSelect.svelte';
    import { walter_selection } from '$walter/services/requests';
    import type { WalterAccountEntry } from '$walter/lib';

    export let entry: Partial<WalterAccountEntry> = {};
    export let fetchImpl: typeof fetch;

    const kontakte = walter_selection.kontakte(fetchImpl);
</script>

<Row>
    <WalterTextInput
        required
        labelText="Nutzername"
        bind:value={entry.username}
    />
</Row>
<Row>
    <WalterTextInput required labelText="Anzeigename" bind:value={entry.name} />
</Row>

<Row>
    <WalterMultiSelect
        titleText="Verlinkte Kontakte"
        value={entry.kontakte}
        entries={kontakte}
    />
</Row>
