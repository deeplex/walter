<script lang="ts">
  import {
    HeaderGlobalAction,
    HeaderUtilities
  } from 'carbon-components-svelte';
  import { Save, TrashCan } from 'carbon-icons-svelte';

  import { WalterAnhaenge, WalterHeader } from '$WalterComponents';
  import { walter_delete, walter_put } from '$WalterServices/requests';
  import { openModal } from '$WalterStore';
  import type { WalterS3File } from '$WalterTypes';
  import { WalterToastContent } from '$WalterLib';

  export let title: Promise<string> | string = 'Saverwalter';
  export let a: any;
  export let apiURL: string;
  export let S3URL: string;
  export let files: WalterS3File[] | undefined = undefined;

  const PutToast = new WalterToastContent(
    'Speichern erfolgreich',
    'Speichern fehlgeschlagen',
    (a: any) => `${title} erfolgreich gespeichert.`,
    (a: any) =>
      `Folgende Einträge sind erforderlich:
			${Object.keys(a.errors)
        .map((e) => e.split('.').pop())
        .join(', \n')}`
  );

  function click_save() {
    walter_put(apiURL, a, PutToast);
  }

  const DeleteToast = new WalterToastContent(
    'Löschen erfolgreich',
    'Löschen fehlgeschlagen',
    (a: any) => `${title} erfolgreich gelöscht.`,
    (a: any) => ''
  );

  function click_delete(title: string) {
    const content = `Bist du sicher, dass du ${title} löschen möchtest?
    	Dieser Vorgang kann nicht rückgängig gemacht werden.`;

    openModal({
      modalHeading: 'Löschen',
      content,
      danger: true,
      primaryButtonText: 'Löschen',
      submit: () =>
        walter_delete(apiURL, DeleteToast).then(() => history.back())
    });
  }
</script>

<WalterHeader {title}>
  {#await title then x}
    <HeaderUtilities>
      <HeaderGlobalAction on:click={click_save} icon={Save} />
      <HeaderGlobalAction on:click={() => click_delete(x)} icon={TrashCan} />
    </HeaderUtilities>
    {#if files}
      <WalterAnhaenge {S3URL} bind:files />
    {/if}
  {/await}
</WalterHeader>
