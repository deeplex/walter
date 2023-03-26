<script>
	import { page } from '$app/stores';
	import { WalterHeader } from '$WalterComponents';
	import { Content } from 'carbon-components-svelte';
</script>

<WalterHeader title="Fehler" />
<Content>
	<h1 style="text-align: center; margin-top: 40vh">
		{$page.status}: {$page.error?.message}
	</h1>
</Content>
