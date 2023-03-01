<script lang="ts">
	import { WalterDataWrapper } from '$components';
	import type { MietminderungEntry } from '$types';

	const headers = [
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' },
		{ key: 'minderung', value: 'Minderung' }
	];

	export let rows: Promise<MietminderungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {rows} {headers} />
