<script lang="ts">
	import {
		FileUploader,
		HeaderAction,
		HeaderPanelDivider,
		HeaderPanelLink,
		HeaderPanelLinks,
		Truncate
	} from 'carbon-components-svelte';

	import { walter_s3_get, walter_s3_post } from '$WalterServices/s3';
	import { page } from '$app/stores';
	import { WalterPreview } from '$WalterComponents';

	export let fileNames: string[];

	let fileUploadComplete: boolean = false;
	let newFiles: File[] = [];

	async function upload() {
		fileUploadComplete = false;
		for (const file of newFiles) {
			{
				walter_s3_post(file, $page.url.pathname).then(() => {
					fileUploadComplete = true;
					if (!fileNames.includes(file.name)) {
						fileNames = [...fileNames, file.name];
					}
				});
			}
		}
	}

	async function showModal(e: MouseEvent) {
		selectedFileName = (e!.target as any).textContent;
		walter_s3_get(`${$page.url.pathname}/${selectedFileName}`).then(
			(e: Blob) => {
				selectedFile = e;
				previewOpen = true;
			}
		);
	}

	let selectedFile: Blob | undefined = undefined;
	let selectedFileName: string = '';
	let previewOpen = false;
</script>

<WalterPreview
	bind:url={$page.url.pathname}
	bind:name={selectedFileName}
	bind:blob={selectedFile}
	bind:open={previewOpen}
/>

<HeaderAction text="({fileNames.length})">
	<HeaderPanelLinks>
		<FileUploader
			status={fileUploadComplete ? 'complete' : 'uploading'}
			bind:files={newFiles}
			on:add={upload}
			multiple
			buttonLabel="Datei hochladen"
		/>
		<HeaderPanelDivider>Dateien ({fileNames.length})</HeaderPanelDivider>
		<HeaderPanelLinks>
			{#each fileNames as fileName}
				<HeaderPanelLink on:click={showModal}>
					<!-- Copy the style from the original element. -->
					<Truncate
						style="font-size: 0.875rem;
							   margin-left: 0;
							   font-weight: 600;
							   line-height: 1.28572;
							   letter-spacing: 0.16px;
							   display: block;
							   height: 2rem;
							   color: #c6c6c6;
							   text-decoration: none;"
					>
						{fileName}
					</Truncate>
				</HeaderPanelLink>
			{/each}
		</HeaderPanelLinks>
	</HeaderPanelLinks>
</HeaderAction>
