<script lang="ts">
	import { Tile } from 'carbon-components-svelte';
	import { onMount } from 'svelte';
	import type { WalterS3File } from '../../types/WalterS3File.type';

	export let file: WalterS3File;
	let text: string = '';

	onMount(() => {
		const reader = new FileReader();
		reader.onload = function (event) {
			text = (event.target?.result as string) || '';
		};
		if (file.Blob) {
			reader.readAsText(file.Blob);
		}

		// TODO may want to catch !file.Blob here.
	});
</script>

<Tile light>{text}</Tile>
