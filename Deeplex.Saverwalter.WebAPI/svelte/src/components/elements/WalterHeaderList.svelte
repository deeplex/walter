<script lang="ts">
  import { Add } from 'carbon-icons-svelte';
  import {
    HeaderGlobalAction,
    HeaderUtilities
  } from 'carbon-components-svelte';

  import { goto } from '$app/navigation';

  import { WalterHeader } from '$WalterComponents';
  import { page } from '$app/stores';

  export let title: string = 'Saverwalter';

  function click_new() {
    goto(`${$page.url.pathname}/new`);
  }
</script>

<WalterHeader {title}>
  <HeaderUtilities>
    <HeaderGlobalAction on:click={click_new} icon={Add} />
  </HeaderUtilities>
</WalterHeader>
