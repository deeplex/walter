<script lang="ts">
    import {
        WalterGrid,
        WalterHeaderNew,
        WalterKontakt,
    } from '$walter/components';
    import type { PageData } from './$types';
    import {
        WalterKontaktEntry
    } from '$walter/lib';

    export let data: PageData;

    const title = 'Neue Person';

    const entry: Partial<WalterKontaktEntry> = {};
</script>

<WalterHeaderNew
    apiURL={WalterKontaktEntry.ApiURL}
    {title}
    {entry} />

<WalterGrid>
    <WalterKontakt {entry} fetchImpl={data.fetchImpl} />
</WalterGrid>
