<script lang="ts">
  import { isWalterSideNavOpen } from '$WalterStore';
  import { Button, Header } from 'carbon-components-svelte';
  import { Close, Menu } from 'carbon-icons-svelte';

  export let title: string = 'Saverwalter';

  let isSideNavOpen = true;

  isWalterSideNavOpen.subscribe((value) => {
    isSideNavOpen = value;
  });

  function sideNavClick() {
    isWalterSideNavOpen.update((value) => !value);
  }
</script>

<Header platformName={title}>
  <svelte:fragment slot="skip-to-content">
    <Button
      style="background-color: #161616"
      iconDescription="Navigation"
      tooltipPosition="right"
      kind="secondary"
      icon={isSideNavOpen ? Close : Menu}
      on:click={sideNavClick}
    />
  </svelte:fragment>
  <slot />
</Header>
