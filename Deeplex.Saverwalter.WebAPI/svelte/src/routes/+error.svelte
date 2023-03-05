<script>
	import { page } from '$app/stores';
</script>

<h1>Hello this is a properly working error page</h1>
<h1>{$page.status}: {$page.error?.message}</h1>
