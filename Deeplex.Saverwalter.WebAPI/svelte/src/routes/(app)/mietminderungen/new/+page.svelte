<script lang="ts">
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterMietminderung
    } from '$walter/components';
    import type { WalterMieteEntry } from '$walter/lib';

    const apiURL = `/api/miete`;
    const title = 'Neue Miete';

    const entry: Partial<WalterMieteEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
    <WalterMietminderung {entry} />
</WalterGrid>
