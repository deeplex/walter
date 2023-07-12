<script lang="ts">
    import {
        WalterGrid,
        WalterErhaltungsaufwendung,
        WalterHeaderNew
    } from '$walter/components';
    import type { WalterErhaltungsaufwendungEntry } from '$walter/lib';
    import type { PageData } from './$types';

    export let data: PageData;
    const entry: Partial<WalterErhaltungsaufwendungEntry> = {};
</script>

<WalterHeaderNew apiURL={data.apiURL} {entry} title={data.title} />

<WalterGrid>
    <WalterErhaltungsaufwendung fetchImpl={data.fetchImpl} {entry} />
</WalterGrid>
