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
	import type { WalterS3File } from '../../types/WalterS3File.type';

	let fileUploadComplete: boolean = false;
	export let files: WalterS3File[];
	let newFiles: File[] = [];

	async function upload() {
		fileUploadComplete = false;
		for (const file of newFiles) {
			{
				walter_s3_post(file, $page.url.pathname).then(() => {
					fileUploadComplete = true;
					files = [
						...files,
						{
							FileName: file.name,
							Key: 'TODO',
							LastModified: file.lastModified,
							Size: file.size
						}
					];
				});
			}
		}
	}

	async function showModal(e: MouseEvent) {
		selectedFileName = (e!.target as any).textContent;
		walter_s3_get(`${$page.url.pathname}/${selectedFileName}`).then((e) => {
			selectedFile = e;
			previewOpen = true;
		});
	}

	let selectedFile: Blob | undefined = undefined;
	let selectedFileName: string = '';
	let previewOpen = false;
</script>

<WalterPreview
	bind:name={selectedFileName}
	bind:blob={selectedFile}
	bind:open={previewOpen}
/>

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
			{#each files as file}
				<HeaderPanelLink on:click={showModal}>
					<Truncate
						style="font-size: 0.875rem;
					margin-left: 0;
					font-weight: 600;
					line-height: 1.28572;
					letter-spacing: 0.16px;
					display: block;
					height: 2rem;
					color: #c6c6c6;
					text-decoration: none;">{file.FileName}</Truncate
					>
				</HeaderPanelLink>
			{/each}
		</HeaderPanelLinks>
	</HeaderPanelLinks>
</HeaderAction>
