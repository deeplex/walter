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

	// TODO show Toast
	async function upload() {
		fileUploadComplete = false;
		for (const file of newFiles) {
			{
				walter_s3_post(file, $page.url.pathname).then(() => {
					fileUploadComplete = true;
					// Don't update if file already exists (file overwrite)
					if (files.some((e) => e.FileName == file.name)) {
						return;
					}
					files = [
						...files,
						{
							FileName: file.name,
							Key: `${$page.url.pathname}/${file.name}`,
							LastModified: file.lastModified,
							Type: file.type,
							Size: file.size
						}
					];
				});
			}
		}
	}

	async function showModal(e: MouseEvent) {
		const name = (e!.target as any).textContent;
		walter_s3_get(`${$page.url.pathname}/${name}`).then((e: Blob) => {
			selectedFile = {
				FileName: name,
				Key: `${$page.url.pathname}/${name}`,
				LastModified: new File([e], '').lastModified,
				Size: e.size,
				Type: e.type,
				Blob: e
			};
			previewOpen = true;
		});
	}

	let selectedFile: WalterS3File;
	let previewOpen = false;
</script>

{#if selectedFile}
	<WalterPreview bind:file={selectedFile} bind:open={previewOpen} />
{/if}

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
						{file.FileName}
					</Truncate>
				</HeaderPanelLink>
			{/each}
		</HeaderPanelLinks>
	</HeaderPanelLinks>
</HeaderAction>
