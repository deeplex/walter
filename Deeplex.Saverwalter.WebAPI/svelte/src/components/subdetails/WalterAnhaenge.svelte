<script lang="ts">
	import {
		FileUploader,
		HeaderAction,
		HeaderPanelDivider,
		HeaderPanelLink,
		HeaderPanelLinks
	} from 'carbon-components-svelte';

	import type { WalterAnhangEntry } from '$WalterTypes';
	import {
		download_file_blob,
		walter_s3_get,
		walter_s3_post
	} from '$WalterServices/s3';
	import { page } from '$app/stores';

	export let rows: WalterAnhangEntry[] = [];
	let fileUploadComplete: boolean = false;
	export let files: string[];
	let newFiles: File[] = rows.map((e) => new File([], e.fileName));

	async function upload() {
		fileUploadComplete = false;
		for (const file of newFiles) {
			{
				walter_s3_post(file, $page.url.pathname).then(() => {
					fileUploadComplete = true;
					files = [...files, file.name];
				});
			}
		}
	}

	async function download(e: MouseEvent) {
		const name = (e!.target as any).text;
		walter_s3_get(`${$page.url.pathname}/${name}`).then((e) =>
			download_file_blob(e, name)
		);
	}
</script>

<HeaderAction text="({files.length})">
	<HeaderPanelLinks>
		<FileUploader
			status={fileUploadComplete ? 'complete' : 'uploading'}
			bind:files={newFiles}
			on:add={upload}
			multiple
			buttonLabel="Datei hochladen"
		/>
		<HeaderPanelDivider>Dateien ({files.length})</HeaderPanelDivider>
		<HeaderPanelLinks>
			{#each files as row}
				<HeaderPanelLink on:click={download}>{row}</HeaderPanelLink>
			{/each}
		</HeaderPanelLinks>
	</HeaderPanelLinks>
</HeaderAction>
