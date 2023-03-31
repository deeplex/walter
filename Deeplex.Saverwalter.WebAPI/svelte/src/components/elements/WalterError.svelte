<script>
	import { page } from '$app/stores';
	import { Content } from 'carbon-components-svelte';
</script>

<Content>
	<h1 style="text-align: center; margin-top: 40vh">
		{$page.status}: {$page.error?.message}
	</h1>
</Content>
