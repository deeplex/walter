<script lang="ts">
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterVertragVersion
    } from '$walter/components';
    import type { WalterMieteEntry } from '$walter/lib';

    const apiURL = `/api/vertragversionen`;
    const title = 'Vertragsänderung';

    const entry: Partial<WalterMieteEntry> = {};
</script>

<WalterHeaderNew {apiURL} {entry} {title} />

<WalterGrid>
    <WalterVertragVersion {entry} />
</WalterGrid>
