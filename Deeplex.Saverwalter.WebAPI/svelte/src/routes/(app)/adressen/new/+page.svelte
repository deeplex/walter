<script lang="ts">
    import {
        WalterAdresse,
        WalterGrid,
        WalterHeaderNew,
        WalterTextArea
    } from '$walter/components';
    import { WalterAdresseEntry } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    const title = 'Neue Adresse';

    const entry: Partial<WalterAdresseEntry> = {};
</script>

<WalterHeaderNew apiURL={WalterAdresseEntry.ApiURL} {entry} {title} />

<WalterGrid>
    <WalterAdresse {entry} />
    <Row>
        <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
    </Row>
</WalterGrid>
