<script lang="ts">
  import {
    WalterHeaderDetail,
    WalterGrid,
    WalterZaehlerstand
  } from '$WalterComponents';
  import { convertDate } from '$WalterServices/utils';
  import { Button, ButtonSkeleton } from 'carbon-components-svelte';
  import type { PageData } from './$types';

  export let data: PageData;
</script>

<WalterHeaderDetail
  S3URL={data.S3URL}
  files={data.anhaenge}
  a={data.a}
  apiURL={data.apiURL}
  title={data.a.zaehler.text + ' - ' + convertDate(new Date(data.a.datum))}
  f={data.fetch}
/>

<WalterGrid>
  <WalterZaehlerstand a={data.a} />
  {#await data.a}
    <ButtonSkeleton />
  {:then x}
    <Button href={`/zaehler/${x.zaehler.id}`}>Zum Zähler</Button>
  {/await}
</WalterGrid>
