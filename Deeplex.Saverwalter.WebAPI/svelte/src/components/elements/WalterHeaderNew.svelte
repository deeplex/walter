<script lang="ts">
  import { HeaderGlobalAction, HeaderNav } from 'carbon-components-svelte';
  import { Save } from 'carbon-icons-svelte';

  import { WalterHeader } from '$WalterComponents';
  import { walter_post } from '$WalterServices/requests';
  import { goto } from '$app/navigation';
  import { WalterToastContent } from '$WalterLib';

  export let title: string = 'Neu...';
  export let apiURL: string;
  export let entry: any;

  const SaveToast = new WalterToastContent(
    'Speichern erfolgreich',
    'Speichern fehlgeschlagen',
    (a: any) => a,
    (a: any) =>
      `Speichern fehlgeschlagen.
			Folgende Einträge sind erforderlich:
			${Object.keys(a.errors)
        .map((e) => e.split('.').pop())
        .join(', \n')}`
  );

  async function click_post() {
    const j = await walter_post(apiURL, entry, SaveToast);
    if (j.id) {
      goto(`${apiURL}/${j.id}`.replace('api/', ''));
    }
  }
</script>

<WalterHeader {title}>
  <slot />
  <HeaderNav>
    <HeaderGlobalAction on:click={click_post} icon={Save} />
  </HeaderNav>
</WalterHeader>
