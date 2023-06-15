<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterMietminderung
    } from '$walter/components';
    import { Button, ButtonSkeleton } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.anhaenge}
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.vertrag.text}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterMietminderung entry={data.entry} />
    {#await data.entry}
        <ButtonSkeleton />
    {:then x}
        <Button href={`/vertraege/${x.vertrag.id}`}>Zum Vertrag</Button>
    {/await}
</WalterGrid>
