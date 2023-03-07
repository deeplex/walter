<script lang="ts">
	import {
		FileUploader,
		HeaderAction,
		HeaderPanelDivider,
		HeaderPanelLink,
		HeaderPanelLinks,
		Loading
	} from 'carbon-components-svelte';

	import type { IWalterAnhang, WalterAnhangEntry } from '$WalterTypes';
	import { walter_s3_post } from '$WalterServices/s3';

	export let reference: IWalterAnhang;
	export let rows: WalterAnhangEntry[];
	let fileUploadComplete: boolean = false;
	let files: File[] = [];

	async function upload() {
		fileUploadComplete = false;
		for (const file of files) {
			{
				walter_s3_post(file, reference).then(() => {
					fileUploadComplete = true;
				});
			}
		}
	}
</script>

{#await rows}
	<HeaderAction disabled>
		<svelte:fragment slot="text">
			<Loading style="margin-left: 1em;" withOverlay={false} small />
		</svelte:fragment>
	</HeaderAction>
{:then x}
	<HeaderAction text="({x.length})">
		<HeaderPanelLinks>
			<FileUploader
				status={fileUploadComplete ? 'complete' : 'uploading'}
				bind:files
				on:add={upload}
				multiple
				buttonLabel="Datei hochladen"
			/>
			<HeaderPanelDivider>Dateien ({x.length})</HeaderPanelDivider>
			<HeaderPanelLinks>
				{#each x as row}
					<HeaderPanelLink>{row.fileName}</HeaderPanelLink>
				{/each}
			</HeaderPanelLinks>
		</HeaderPanelLinks>
	</HeaderAction>
{/await}
