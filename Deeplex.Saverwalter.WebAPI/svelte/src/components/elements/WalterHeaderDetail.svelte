<script lang="ts">
	import {
		HeaderGlobalAction,
		HeaderNav,
		Loading
	} from 'carbon-components-svelte';
	import { Save, TrashCan } from 'carbon-icons-svelte';

	import { WalterAnhaenge, WalterHeader } from '$WalterComponents';
	import { walter_delete, walter_put } from '$WalterServices/requests';
	import { openModal } from '$WalterStore';
	import type { WalterS3File } from '$WalterTypes';

	export let title: Promise<string> | string = 'Saverwalter';
	export let a: any;
	export let apiURL: string;
	export let S3URL: string;
	export let files: WalterS3File[] | undefined = undefined;

	function click_save() {
		walter_put(apiURL, a);
	}

	function click_delete(title: string) {
		const content = `Bist du sicher, dass du ${title} löschen möchtest?
    	Dieser Vorgang kann nicht rückgängig gemacht werden.`;

		openModal({
			modalHeading: 'Löschen',
			content,
			danger: true,
			primaryButtonText: 'Löschen',
			submit: () => walter_delete(apiURL).then(() => history.back())
		});
	}
</script>

<WalterHeader {title}>
	{#await title then x}
		<HeaderNav>
			{#await a}
				<HeaderGlobalAction>
					<Loading style="margin-left: 33%" withOverlay={false} small />
				</HeaderGlobalAction>
				<HeaderGlobalAction>
					<Loading style="margin-left: 33%" withOverlay={false} small />
				</HeaderGlobalAction>
			{:then}
				<HeaderGlobalAction on:click={click_save} icon={Save} />
				<HeaderGlobalAction on:click={() => click_delete(x)} icon={TrashCan} />
			{/await}
		</HeaderNav>
		{#if files}
			<WalterAnhaenge {S3URL} bind:files />
		{/if}
	{/await}
</WalterHeader>
