<script lang="ts">
	import { WalterDataWrapper } from '$WalterComponents';
	import type { WalterVertragVersionEntry } from '$WalterTypes';

	const headers = [
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'personenzahl', value: 'Personenzahl' },
		{ key: 'grundmiete', value: 'Grundmiete' }
	];

	export let rows: Promise<WalterVertragVersionEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
