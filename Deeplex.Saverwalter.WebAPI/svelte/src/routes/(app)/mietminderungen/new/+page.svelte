<script lang="ts">
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterMiete
    } from '$walter/components';
    import type { WalterMieteEntry } from '$walter/lib';

    const apiURL = `/api/miete`;
    const title = 'Neue Miete';

    const entry: Partial<WalterMieteEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
    <WalterMiete a={entry} />
</WalterGrid>
