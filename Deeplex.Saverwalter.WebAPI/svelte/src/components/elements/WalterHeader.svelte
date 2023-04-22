<script lang="ts">
  import { Header, Loading, SkipToContent } from 'carbon-components-svelte';

  export let title: Promise<string> | string = 'Saverwalter';
</script>

{#await title}
  <Header href="/" platformName="Lade...">
    <Loading withOverlay={false} small />
  </Header>
{:then x}
  <Header href="/" platformName={x}>
    <slot />
  </Header>
{/await}
