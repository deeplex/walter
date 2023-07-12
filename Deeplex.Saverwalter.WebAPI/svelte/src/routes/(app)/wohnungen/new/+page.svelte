<script lang="ts">
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterWohnung
    } from '$walter/components';
    import type { WalterWohnungEntry } from '$walter/lib';
    import type { PageData } from './$types';

    export let data: PageData;
    const entry: Partial<WalterWohnungEntry> = {};
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterWohnung fetchImpl={data.fetchImpl} {entry} />
</WalterGrid>
