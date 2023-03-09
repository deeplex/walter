<script lang="ts">
	import { WalterHeader, WalterNumberInput } from '$WalterComponents';
	import { walter_s3_post } from '$WalterServices/s3';
	import { Button, Content } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	export let data: PageData;

	let jahr: number = new Date().getFullYear() - 1;

	const headers = {
		'Content-Type': 'application/octet-stream'
	};

	function click() {
		const url = `/api/betriebskostenabrechnung/${data.id}/${jahr}`;
		fetch(url, {
			method: 'GET',
			headers
		})
			.then((e) => e.blob())
			.then((e) =>
				walter_s3_post(
					new File([e], `Abrechnung ${jahr}.docx`),
					`vertraege/${data.id}`
				)
			);
	}
</script>

<WalterHeader title="Betriebskostenrechnung" />

<Content>
	<WalterNumberInput bind:value={jahr} label="Jahr" hideSteppers={false} />
	<Button on:click={click}>Betriebskostenabrechnung erstellen</Button>
</Content>
