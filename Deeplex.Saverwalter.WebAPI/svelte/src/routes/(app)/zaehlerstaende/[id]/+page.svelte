<script lang="ts">
    import {
        WalterHeaderDetail,
        WalterGrid,
        WalterZaehlerstand
    } from '$walter/components';
    import { convertDateGerman } from '$walter/services/utils';
    import { Button, ButtonSkeleton } from 'carbon-components-svelte';
    import type { PageData } from './$types';

    export let data: PageData;
</script>

<WalterHeaderDetail
    S3URL={data.S3URL}
    files={data.anhaenge}
    entry={data.entry}
    apiURL={data.apiURL}
    title={data.entry.zaehler.text +
        ' - ' +
        convertDateGerman(new Date(data.entry.datum))}
    fetchImpl={data.fetch}
/>

<WalterGrid>
    <WalterZaehlerstand entry={data.entry} />
    {#await data.entry}
        <ButtonSkeleton />
    {:then x}
        <Button href={`/zaehler/${x.zaehler.id}`}>Zum Zähler</Button>
    {/await}
</WalterGrid>
