<script lang="ts">
    import {
        WalterAdresse,
        WalterGrid,
        WalterHeaderNew
    } from '$walter/components';
    import type { WalterAdresseEntry } from '$walter/lib';

    const apiURL = `/api/adressen`;
    const title = 'Neue Adresse';

    const entry: Partial<WalterAdresseEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
    <WalterAdresse {entry} />
</WalterGrid>
