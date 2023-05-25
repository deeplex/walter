<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterMiete
    } from '$walter/components';
    import { Button, ButtonSkeleton } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.anhaenge}
    a={data.a}
    apiURL={data.apiURL}
    title={data.a.vertrag.text}
    f={data.fetch}
/>

<WalterGrid>
    <WalterMiete a={data.a} />
    {#await data.a}
        <ButtonSkeleton />
    {:then x}
        <Button href={`/vertraege/${x.vertrag.id}`}>Zum Vertrag</Button>
    {/await}
</WalterGrid>
