<script lang="ts">
    import {
        WalterAdresse,
        WalterGrid,
        WalterHeaderNew,
        WalterTextArea
    } from '$walter/components';
    import type { WalterAdresseEntry } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    const apiURL = `/api/adressen`;
    const title = 'Neue Adresse';

    const entry: Partial<WalterAdresseEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
    <WalterAdresse {entry} />
    <Row>
        <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
    </Row>
</WalterGrid>
