<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { WalterHeader } from '$WalterComponents';
	import { Content } from 'carbon-components-svelte';
	import { onMount } from 'svelte';

	onMount(() => {
		const unsubscribe = page.subscribe((value) => {
			if (value.status === 500) {
				console.log('hi');
				goto('/login');
			}
		});

		return () => {
			unsubscribe();
		};
	});
</script>

<WalterHeader title="Fehler" />
<Content>
	<h1 style="text-align: center; margin-top: 40vh">
		{$page.status}: {$page.error?.message}
	</h1>
</Content>
